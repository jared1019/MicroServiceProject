using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MicroService.Framework.JWTExtend
{
    /// <summary>
    /// 对称可逆加密
    /// </summary>
    public class JWTHSService : IJWTService
    {
        private static Dictionary<string, JWTUserModel> TokenCache = new Dictionary<string, JWTUserModel>();

        #region Option注入
        private readonly JWTTokenOptions _JWTTokenOptions;
        public JWTHSService(IOptionsMonitor<JWTTokenOptions> jwtTokenOptions)
        {
            _JWTTokenOptions = jwtTokenOptions.CurrentValue;
        }
        #endregion

        public string GetToken(JWTUserModel userModel)
        {
            return IssueToken(userModel);
        }

        /// <summary>
        /// 刷新token的有效期问题上端校验
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public string GetTokenByRefresh(string refreshToken)
        {
            //refreshToken在有效期，但是缓存可能没有？ 还能去手动清除--比如改密码了，清除缓存，用户来刷新token就发现没有了，需要重新登陆
            if (TokenCache.ContainsKey(refreshToken))
            {
                string token = IssueToken(TokenCache[refreshToken], 60);
                return token;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 2个token  就是有效期不一样
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public Tuple<string, string> GetTokenWithRefresh(JWTUserModel userInfo)
        {
            string token = IssueToken(userInfo, 60);//1分钟
            string refreshToken = IssueToken(userInfo, 60 * 60 * 24 * 7);//7*24小时
            TokenCache.Add(refreshToken, userInfo);

            return Tuple.Create(token, refreshToken);
        }

        //public bool ValidateTokenExpire(string token)
        //{
        //    try
        //    {
        //        IJsonSerializer serializer = new JsonNetSerializer();
        //        IDateTimeProvider provider = new UtcDateTimeProvider();
        //        IJwtValidator validator = new JwtValidator(serializer, provider);
        //        IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        //        IJwtAlgorithm alg = new HMACSHA256Algorithm();
        //        IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, alg);
        //        var json = decoder.Decode(token);
        //        //校验通过，返回解密后的字符串
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        private string IssueToken(JWTUserModel userModel, int second = 600)
        {
            var claims = new[]
            {
                   new Claim(ClaimTypes.Name, userModel.UserName),
                   new Claim(ClaimTypes.MobilePhone, userModel.Phone),
                   new Claim("Id", userModel.Id.ToString()),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JWTTokenOptions.SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            /**
             * Claims (Payload)
                Claims 部分包含了一些跟这个 token 有关的重要信息。 JWT 标准规定了一些字段，下面节选一些字段:
                iss: The issuer of the token，token 是给谁的
                sub: The subject of the token，token 主题
                exp: Expiration Time。 token 过期时间，Unix 时间戳格式
                iat: Issued At。 token 创建时间， Unix 时间戳格式
                jti: JWT ID。针对当前 token 的唯一标识
                除了规定的字段外，可以包含其他任何 JSON 兼容的字段。
             * */
            var token = new JwtSecurityToken(
                issuer: _JWTTokenOptions.Issuer,
                audience: _JWTTokenOptions.Audience,
                claims: claims,
                expires: DateTime.Now.AddSeconds(second),//10分钟有效期
                notBefore: DateTime.Now,//立即生效  DateTime.Now.AddMilliseconds(30),//30s后有效
                signingCredentials: creds);
            string returnToken = new JwtSecurityTokenHandler().WriteToken(token);
            return returnToken;
        }
    }
}
