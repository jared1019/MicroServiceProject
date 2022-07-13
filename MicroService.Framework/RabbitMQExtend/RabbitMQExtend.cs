using AgileFramework.Core;
using Microsoft.Extensions.DependencyInjection;

namespace MicroService.Core.RabbitMQ
{
    public static class RabbitMQExtend
    {
        /// <summary>
        /// 完成注册
        /// </summary>
        /// <param name="services"></param>
        public static void AddRabbitMQRegister(this IServiceCollection services, Action<RabbitMQOptions> action)
        {
            services.Configure<RabbitMQOptions>(action);
            services.AddScoped<RabbitMQInvoker>();
        }
    }
}
