using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Host.ConfigureAppConfiguration(conf =>
{
    conf.AddJsonFile("configuration.json", optional: false, reloadOnChange: true);
});
builder.Services.AddEndpointsApiExplorer();
#region Swagger
builder.Services.AddSwaggerGen(options =>
{
    #region Swagger配置支持Token参数传递 
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "请输入token,格式为 Bearer jwtToken(注意中间必须有空格)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });//添加安全定义
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {   //添加安全要求
                    new OpenApiSecurityScheme
                    {
                        Reference =new OpenApiReference()
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id ="Bearer"
                        }
                    },
                    new string[]{ }
                }
                });
    #endregion
});

builder.Services.AddControllers();
#endregion

builder.Services.AddOcelot();

var app = builder.Build();

#region Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/auth/swagger/v1/swagger.json", "鉴权 API V1");

    //c.SwaggerEndpoint("/auth/swagger/v1/swagger.json", "鉴权 API V1");
    //c.SwaggerEndpoint("/auth/swagger/v1/swagger.json", "AuthenticationCenter  WebAPI V1");
    c.SwaggerEndpoint("/user/swagger/v1/swagger.json", "用户 API V1");
});
#endregion

app.UseOcelot();
app.Run();
