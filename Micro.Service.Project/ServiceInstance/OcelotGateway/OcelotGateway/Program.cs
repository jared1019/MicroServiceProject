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
    #region Swagger����֧��Token�������� 
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "������token,��ʽΪ Bearer jwtToken(ע���м�����пո�)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });//��Ӱ�ȫ����
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {   //��Ӱ�ȫҪ��
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
    c.SwaggerEndpoint("/auth/swagger/v1/swagger.json", "��Ȩ API V1");

    //c.SwaggerEndpoint("/auth/swagger/v1/swagger.json", "��Ȩ API V1");
    //c.SwaggerEndpoint("/auth/swagger/v1/swagger.json", "AuthenticationCenter  WebAPI V1");
    c.SwaggerEndpoint("/user/swagger/v1/swagger.json", "�û� API V1");
});
#endregion

app.UseOcelot();
app.Run();
