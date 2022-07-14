using Autofac;
using Autofac.Extras.DynamicProxy;
using MicroService.Repository;
using System.Reflection;

namespace MicroService.Framework.ServiceExtensions
{
    public class AutofacModuleRegister : Autofac.Module
    {
        private readonly string _assemblyName;

        public AutofacModuleRegister(string assemblyName)
        {
            _assemblyName = assemblyName;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(BaseRepository<>)).As(typeof(IBaseRepository<>)).InstancePerDependency();
            var cacheType = new List<Type>();

            builder.RegisterAssemblyTypes(GetAssemblyByName($"{_assemblyName}.Service"))
                .AsImplementedInterfaces()
                .InstancePerDependency()
                .PropertiesAutowired()
                .EnableInterfaceInterceptors()
                .InterceptedBy(cacheType.ToArray());

            builder.RegisterAssemblyTypes(GetAssemblyByName($"MicroService.Repository"))
                .AsImplementedInterfaces()
                .InstancePerDependency()
                .PropertiesAutowired();
        }

        /// <summary>
        /// 根据程序集名称获取程序集
        /// </summary>
        /// <param name="AssemblyName">程序集名称</param>
        public static Assembly GetAssemblyByName(string AssemblyName)
        {
            return Assembly.Load(AssemblyName);
        }
    }
}
