using MicroService.Common;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace MicroService.Framework.Middleware
{
    /// <summary>
    /// 生成数据库和表
    /// </summary>
    public static class GenerateDataMiddleware
    {
        public static void UseGenerateDataMiddle(this IApplicationBuilder app, MyContext myContext, string webRootPath)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            try
            {
                if (Appsettings.app("AppSettings", "SeedDBEnabled").ObjToBool() || Appsettings.app("AppSettings", "SeedDBDataEnabled").ObjToBool())
                {
                    DBCreate.Async(myContext, webRootPath).Wait();
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error($"Operation database error.\n{e.Message}");
                throw;
            }
        }
    }
}
