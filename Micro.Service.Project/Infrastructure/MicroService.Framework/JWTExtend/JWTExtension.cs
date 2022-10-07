using MicroService.Framework.JWTExtend.RSA;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Framework.JWTExtend
{
    /// <summary>
    /// 集中注册和配置信息
    /// </summary>
    public static class JWTExtension
    {
        /// <summary>
        /// 完成注册
        /// </summary>
        /// <param name="services"></param>
        public static void AddJWTBuilder(this IServiceCollection services, JWTAlgorithmType algorithmType, Action configureJWTTokenOptions)
        {
            switch (algorithmType)
            {
                case JWTAlgorithmType.HS256:
                    #region HS256 对称可逆加密
                    services.AddScoped<IJWTService, JWTHSService>();
                    configureJWTTokenOptions.Invoke();
                    #endregion
                    break;
                case JWTAlgorithmType.RS256:
                    #region RS256 非对称可逆加密，需要获取一次公钥
                    services.AddScoped<IJWTService, JWTHSService>();

                    string keyDir = Directory.GetCurrentDirectory();
                    if (RSAHelper.TryGetKeyParameters(keyDir, true, out RSAParameters keyParams) == false)
                    {
                        keyParams = RSAHelper.GenerateAndSaveKey(keyDir);
                    }
                    configureJWTTokenOptions.Invoke();
                    #endregion
                    break;
                default:
                    throw new Exception("wrong JWTAlgorithmType");
            }
        }

    }

    public enum JWTAlgorithmType
    {
        HS256,
        RS256
    }
}
