using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroService.Framework.JWTExtend
{
    /// <summary>
    /// 简单封装个注入
    /// </summary>
    public interface IJWTService
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        string GetToken(JWTUserModel userInfo);

        /// <summary>
        /// 获取Token+RefreshToken
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns>Token+RefreshToken</returns>
        Tuple<string, string> GetTokenWithRefresh(JWTUserModel userInfo);

        /// <summary>
        /// 基于refreshToken获取Token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        string GetTokenByRefresh(string refreshToken);

        ///// <summary>
        ///// 检查是否过期
        ///// </summary>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //bool ValidateTokenExpire(string token);
    }
}
