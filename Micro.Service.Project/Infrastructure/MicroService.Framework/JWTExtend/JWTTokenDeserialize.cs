using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Framework.JWTExtend
{
    public static class JWTTokenDeserialize
    {
        /// <summary>
        /// 检测refreshToken
        /// true为可用  false为过期
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool ValidateRefreshToken(this string token)
        {
            try
            {
                string result = token.AnalysisToken();
                if (string.IsNullOrEmpty(result))
                {
                    return false;
                }
                else
                {
                    var jsonResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                    DateTime expiredTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
                    expiredTime = expiredTime.AddSeconds(int.Parse(jsonResult["exp"]));
                    return expiredTime >= DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        private static DateTime ConvertIntDatetime(double utc)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            startTime = startTime.AddSeconds(utc);
            startTime = startTime.AddHours(8);//转化为北京时间(北京时间=UTC时间+8小时 )            
            return startTime;

        }


        /// <summary>
        /// 解析payload字符串
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static string AnalysisToken(this string token)
        {
            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtAlgorithm alg = new HMACSHA256Algorithm();
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, alg);
                var json = decoder.Decode(token);
                //校验通过，返回解密后的字符串
                return json;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

    }
}
