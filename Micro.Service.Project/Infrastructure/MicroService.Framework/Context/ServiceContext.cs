using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroService.Framework.Context
{
    /// <summary>
    /// 应用程序上下文
    /// </summary>
    public static class ServiceContext
    {
        public static IServiceProvider _serviceProvider;
        public static ILifetimeScope _lifetimeScope;

        public static void UseServiceContext(this IApplicationBuilder applicationBuilder)
        {
            _serviceProvider = applicationBuilder.ApplicationServices;
            _lifetimeScope = _serviceProvider.GetAutofacRoot();
        }

        /// <summary>
        /// 获取对象实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetService<T>()
        {
            return _serviceProvider.GetService<T>();
        }

        /// <summary>
        /// 获取Scope对象实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetScopeService<T>()
        {
            var scope = _serviceProvider.CreateScope();
            return scope.ServiceProvider.GetService<T>();
        }

        /// <summary>
        /// 获取对象实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetRequiredService<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// 获取对象实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetOptionsMonitor<T>()
        {
            return _serviceProvider.GetService<IOptionsMonitor<T>>().CurrentValue;
        }

        /// <summary>
        /// 获取对象实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetOptions<T>() where T : class, new()
        {
            return _serviceProvider.GetService<IOptions<T>>().Value;
        }
    }
}