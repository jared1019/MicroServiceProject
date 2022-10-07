using Autofac;
using Autofac.Extensions.DependencyInjection;
using MicroService.Common;
using MicroService.Framework.ServiceExtensions;
using User.GrpcService.Services;

namespace Company.WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Autofac

            {
                builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder.RegisterModule(new AutofacModuleRegister("UserService"));
                });
            }

            #endregion

            builder.Services.AddSingleton(new Appsettings(builder.Configuration));
            builder.Services.AddDbSetup();
            builder.Services.AddSqlsugarSetup();
            builder.Services.AddGrpc();

            var app = builder.Build();


            // Configure the HTTP request pipeline.
            app.MapGrpcService<GreeterService>();
            app.MapGrpcService<UsersService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
    }
}