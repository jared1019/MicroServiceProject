using Microsoft.Extensions.DependencyInjection;

namespace MicroService.Core.HttpExtend
{
    public static class HttpRequestInvokerExtension
    {
        public static void AddHttpInvoker(this IServiceCollection services, Action<HttpRequestInvokerOptions> action)
        {
            services.Configure<HttpRequestInvokerOptions>(action);//配置给IOC  其他字段用默认值

            services.AddTransient<IHttpRequestInvoker, HttpRequestInvoker>();
            //如果还有其他注册，就一并完成
        }

        public static void AddHttpInvoker(this IServiceCollection services)
        {
            services.AddHttpInvoker(null);
        }
    }
}
