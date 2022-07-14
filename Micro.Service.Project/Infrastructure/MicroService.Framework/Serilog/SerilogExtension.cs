using MicroService.Framework.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace MicroService.Framework.Serilog
{
    public static class SerilogExtension
    {
        private static readonly IHttpContextAccessor _contextAccessor = ServiceContext.GetService<IHttpContextAccessor>();

        public static LoggerConfiguration WithTraceIdentifier(
              this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With<CustomEnricher>();
        }

        public static IHostBuilder ConfigureSerilog(this IHostBuilder builder)
        {
            return builder.UseSerilog((ctx, config) =>
                config.Enrich.WithTraceIdentifier().ReadFrom.Configuration(ctx.Configuration)
            );
        }

        public const string SerilogFormat = "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception:j}";

        public static ILogger CreateLogger(IConfiguration configuration)
        {
            LoggerConfiguration loggerConfiguration = new LoggerConfiguration().MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)//重写特定命名空间或类型名称中事件的最低级别。
                    .MinimumLevel.Override("System", LogEventLevel.Debug)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Error)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Error)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information, outputTemplate: SerilogFormat)
                    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(configuration.GetSection("EsConfig").Get<List<string>>().Select(c => new Uri(c)))
                    {
                        AutoRegisterTemplate = true,
                        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                        MinimumLogEventLevel = LogEventLevel.Warning,
                    });
            Log.Logger = loggerConfiguration.CreateLogger();
            return Log.Logger;
        }
    }


    public class CustomEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor? _contextAccessor = null;

        public CustomEnricher()
        {
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ThreadId", Thread.CurrentThread.ManagedThreadId));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ProcessName", System.Diagnostics.Process.GetCurrentProcess().ProcessName));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ProcessId", System.Diagnostics.Process.GetCurrentProcess().Id));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("EnvironmentUserName", GetEnvironmentUserName()));

            if (_contextAccessor != null)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestIP", _contextAccessor.HttpContext?.Connection?.RemoteIpAddress));
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Referer", _contextAccessor.HttpContext?.Request?.Headers["Referer"]));
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestPath", _contextAccessor.HttpContext?.Request?.Path));
            }
        }

        private static string? GetEnvironmentUserName()
        {
#if ENV_USER_NAME
                    var userDomainName = Environment.UserDomainName;
                    var userName = Environment.UserName;
#else
            var userDomainName = Environment.GetEnvironmentVariable("USERDOMAIN");
            var userName = Environment.GetEnvironmentVariable("USERNAME");
#endif
            return !string.IsNullOrWhiteSpace(userDomainName) ? $@"{userDomainName}\{userName}" : userName;
        }
    }
}