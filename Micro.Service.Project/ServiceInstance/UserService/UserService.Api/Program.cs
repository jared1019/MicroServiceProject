using Autofac;
using Autofac.Extensions.DependencyInjection;
using MicroService.Common;
using MicroService.Framework.Context;
using MicroService.Framework.Middleware;
using MicroService.Framework.Serilog;
using MicroService.Framework.ServiceExtensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
SerilogExtension.CreateLogger(builder.Configuration);

#region Autofac

{
    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .UseSerilog()
    .ConfigureContainer<ContainerBuilder>(builder =>
    {
        builder.RegisterModule(new AutofacModuleRegister("UserService"));
    });
}

#endregion

//���÷���
builder.Services.AddSingleton(new Appsettings(builder.Configuration));
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapperSetup();
builder.Services.AddDbSetup();
builder.Services.AddSqlsugarSetup();
builder.Services.AddRedisRegister(option =>
{
    option.Host = builder.Configuration.GetSection("RedisConfig")["Host"];
    option.Prot = Convert.ToInt32(builder.Configuration.GetSection("RedisConfig")["Prot"]);
    option.DB = Convert.ToInt32(builder.Configuration.GetSection("RedisConfig")["DB"]);
    option.Password = builder.Configuration.GetSection("RedisConfig")["Password"];
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
var myContext = scope.ServiceProvider.GetRequiredService<MyContext>();
app.UseGenerateDataMiddle(myContext, builder.Environment.ContentRootPath);

app.UseServiceContext();//ʹ��Ӧ�ó���������
app.UseExceptionMiddleware();//���ȫ���쳣

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
