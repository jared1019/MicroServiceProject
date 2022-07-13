using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Polly;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
{
    configurationBuilder.AddJsonFile("configuration.json", optional: false, reloadOnChange: true);
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo { Title = "Gateway API", Version = "v1", Description = "# gateway api..." });
});

builder.Services.AddControllers();
#endregion

builder.Services.AddOcelot()//Ocelot如何处理
        .AddConsul()//支持Consul
        .AddPolly()
        ;
var app = builder.Build();

// Configure the HTTP request pipeline.
#region Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    //c.SwaggerEndpoint("/auth/swagger/v1/swagger.json", "鉴权 API V1");
    c.SwaggerEndpoint("/user/swagger/v1/swagger.json", "用户 API V1");
});

#endregion
app.UseOcelot();

app.UseAuthorization();

app.MapControllers();

app.Run();
