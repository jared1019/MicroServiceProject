using AgileFramework.Core;
using DotNetCore.CAP;
using DotNetCore.CAP.Messages;
using MicroService.Core;
using MicroService.Core.ConsulExtend;
using MicroService.Core.RabbitMQ;
using MicroService.Core.RedisExtend;
using MicroService.Interface;
using MicroService.Model;
using MicroService.Service;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddTransient<CapUserService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Consul Server IOC注册
builder.Services.Configure<ConsulRegisterOptions>(builder.Configuration.GetSection("ConsulRegisterOptions"));
builder.Services.Configure<ConsulClientOptions>(builder.Configuration.GetSection("ConsulClientOptions"));
builder.Services.AddConsulRegister();
#endregion

#region rabbitMQ注册
builder.Services.AddRabbitMQRegister(option =>
{
    option.HostName = builder.Configuration.GetSection("RabbitMQ")["HostName"];
    option.UserName = builder.Configuration.GetSection("RabbitMQ")["UserName"];
    option.Password = builder.Configuration.GetSection("RabbitMQ")["Password"];
});
#endregion

#region redis注册
builder.Services.AddRedisRegister(option =>
{
    option.Host = builder.Configuration.GetSection("Redis")["Host"];
    option.Prot = Convert.ToInt32(builder.Configuration.GetSection("Redis")["Prot"]);
    option.DB = Convert.ToInt32(builder.Configuration.GetSection("Redis")["DB"]);
    option.Password = builder.Configuration.GetSection("Redis")["Password"];
});
#endregion

builder.Services.AddCap(x =>
{
    x.UseMySql(builder.Configuration.GetConnectionString("Default"));
    x.UseRabbitMQ(opt =>
    {
        opt.HostName = builder.Configuration.GetSection("RabbitMQ")["HostName"];
        opt.UserName = builder.Configuration.GetSection("RabbitMQ")["UserName"];
        opt.Password = builder.Configuration.GetSection("RabbitMQ")["Password"];
    });
    x.FailedRetryCount = 10;
    x.FailedRetryInterval = 60;
    x.FailedThresholdCallback = failed =>
    {
        var logger = failed.ServiceProvider.GetService<ILogger<Program>>();
        logger.LogError($@"MessageType {failed.MessageType} 失败了， 重试了 {x.FailedRetryCount} 次, 
                        消息名称: {failed.Message.GetName()}");
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#region Consul注册
app.UseHealthCheckMiddleware("/Api/Health/Index");//心跳请求响应
app.Services.GetService<IConsulRegister>()!.UseConsulRegist();
#endregion

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           DateTime.Now.AddDays(index),
           Random.Shared.Next(-20, 55),
           summaries[Random.Shared.Next(summaries.Length)]
       ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/api/users/all", (IUserService userService, IHttpContextAccessor httpContextAccessor) =>
{
    var host = httpContextAccessor.HttpContext!.Request.Host;

    return userService.UserAll().Select(u => new User()
    {
        Id = u.Id,
        Account = u.Account + "MA",
        Name = u.Name,
        Role = $"{ app.Configuration["ip"] ?? host.Host}" +
        $"{ app.Configuration["port"] ?? (host.Port is null ? "NonePort" : host.Port!.Value.ToString())}",
        Email = u.Email,
        LoginTime = u.LoginTime,
        Password = u.Password
    });

});

app.MapPost("/api/users/push", (IUserService userService, RabbitMQInvoker rabbitMQInvoker, IHttpContextAccessor httpContextAccessor) =>
{
    try
    {
        var host = httpContextAccessor.HttpContext!.Request.Host;

        var userList = userService.UserAll().Select(u => new User()
        {
            Id = u.Id,
            Account = u.Account + "MA",
            Name = u.Name,
            Role = $"{ app.Configuration["ip"] ?? host.Host}" +
             $"{ app.Configuration["port"] ?? (host.Port is null ? "NonePort" : host.Port!.Value.ToString())}",
            Email = u.Email,
            LoginTime = u.LoginTime,
            Password = u.Password
        });
        rabbitMQInvoker.Send("user.Exchange", JsonConvert.SerializeObject(userList));
        return true;
    }
    catch (Exception ex)
    {
        return false;
    }
});

app.MapPost("/api/users/Cache", (IUserService userService, CacheClientDB cacheClient, IHttpContextAccessor httpContextAccessor) =>
{
    try
    {
        var host = httpContextAccessor.HttpContext!.Request.Host;
        IEnumerable<User> userList = null;
        string cacheKey = "userList";
        if (cacheClient.ContainsKey(cacheKey))
        {
            userList = cacheClient.Get<IEnumerable<User>>(cacheKey);
        }
        else
        {
            userList = userService.UserAll().Select(u => new User()
            {
                Id = u.Id,
                Account = u.Account + "MA",
                Name = u.Name,
                Role = $"{ app.Configuration["ip"] ?? host.Host}" +
                $"{ app.Configuration["port"] ?? (host.Port is null ? "NonePort" : host.Port!.Value.ToString())}",
                Email = u.Email,
                LoginTime = u.LoginTime,
                Password = u.Password
            });
        }
        cacheClient.Set("userList", userList);
        return true;
    }
    catch (Exception ex)
    {
        return false;
    }
});


app.MapPost("/api/users/CAP", (IUserService userService, CacheClientDB cacheClient, ICapPublisher capPublisher, IHttpContextAccessor httpContextAccessor) =>
{
    try
    {
        var host = httpContextAccessor.HttpContext!.Request.Host;
        IEnumerable<User> userList = null;
        string cacheKey = "userList";
        if (cacheClient.ContainsKey(cacheKey))
        {
            userList = cacheClient.Get<IEnumerable<User>>(cacheKey);
        }
        else
        {
            userList = userService.UserAll().Select(u => new User()
            {
                Id = u.Id,
                Account = u.Account + "MA",
                Name = u.Name,
                Role = $"{ app.Configuration["ip"] ?? host.Host}" +
                $"{ app.Configuration["port"] ?? (host.Port is null ? "NonePort" : host.Port!.Value.ToString())}",
                Email = u.Email,
                LoginTime = u.LoginTime,
                Password = u.Password
            });

            if (userList.Count() > 0)
                cacheClient.Set("userList", userList);
        }
        capPublisher.PublishAsync("UserService", userList);
        return true;
    }
    catch (Exception ex)
    {
        return false;
    }
});
app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}