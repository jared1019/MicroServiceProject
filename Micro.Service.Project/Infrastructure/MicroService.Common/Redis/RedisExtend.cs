using Microsoft.Extensions.DependencyInjection;

namespace MicroService.Common
{
    public static class RedisExtend
    {
        /// <summary>
        /// 完成注册
        /// </summary>
        /// <param name="services"></param>
        public static void AddRedisRegister(this IServiceCollection services, Action<RedisOptions> action)
        {
            services.Configure(action);
            services.AddScoped<CacheClientDB>();
        }
    }
}
