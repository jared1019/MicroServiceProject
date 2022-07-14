using MicroService.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MicroService.Framework.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;//下一个中间件
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHttpContextAccessor _contextAccessor;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHttpContextAccessor contextAccessor)
        {
            _next = next;
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/json;charset=utf-8;";
                //无论是否为开发环境都记录错误日志
                //记录异常日志
                string ip = context.Connection.RemoteIpAddress.ToString();
                string message = ex.Message
                               + ex.InnerException
                               ?.InnerException
                               ?.Message
                               + ex.InnerException
                               + ex.StackTrace;
                _logger.LogError($"异常记录=>请求路径:{context.Request.Path}，ip地址:{ip}，异常信息:{message}");
                //浏览器在开发环境显示详细错误信息,其他环境隐藏错误信息
                await HandleException(context, message);
            }
        }

        private async Task HandleException(HttpContext context, string errmsg)
        {
            if (context.Request.IsAjax())
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(MessageModel.Fail(errmsg)));
            }
            else
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(errmsg);
            }
        }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            return app.UseMiddleware<ExceptionMiddleware>();
        }

        /// <summary>
        /// 验证是否是Ajax请求
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static bool IsAjax(this HttpRequest req)
        {
            bool result = false;

            var xreq = req.Headers.ContainsKey("x-requested-with");
            if (xreq)
            {
                result = req.Headers["x-requested-with"] == "XMLHttpRequest";
            }

            return result;
        }
    }
}