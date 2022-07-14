using MicroService.Common;
using Microsoft.Extensions.DependencyInjection;

namespace MicroService.Framework.ServiceExtensions
{
    /// <summary>
    /// 启动服务
    /// </summary>
    public static class DbSetup
    {
        public static void AddDbSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddScoped<DBCreate>();
            services.AddScoped<MyContext>();
        }
    }
}
