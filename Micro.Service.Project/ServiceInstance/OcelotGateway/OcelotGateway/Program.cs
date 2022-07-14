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
builder.Services.AddSwaggerGen();
#region Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo { Title = "Gateway API", Version = "v1", Description = "# gateway api..." });
});

builder.Services.AddControllers();
#endregion

builder.Services.AddOcelot();

var app = builder.Build();

#region Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    //c.SwaggerEndpoint("/auth/swagger/v1/swagger.json", "¼øÈ¨ API V1");
    c.SwaggerEndpoint("/user/swagger/v1/swagger.json", "ÓÃ»§ API V1");
});
#endregion

app.UseOcelot();
app.Run();
