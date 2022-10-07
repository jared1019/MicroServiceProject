using MicroService.Common;
using MicroService.Model;
using Microsoft.AspNetCore.Mvc;
using UserService.Interface;
using UserService.Model.Models;

namespace UserService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseApiController
    {
        private readonly IUserServices _userService;

        public UserController(IUserServices userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// 检查信息格式
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [Route("check/{data}/{type}")]
        [HttpGet]
        public MessageModel<int> CheckData(string data, int type)
        {

            int exist = 0;
            switch (type)
            {
                case 1:
                    var xx = _userService.Query(c => c.Username == "admin").Result.ToList();
                    exist = _userService.Query(c => c.Username.Equals(data)).Result.Count();
                    string msg = exist == 0 ? "校验成功" : "校验失败，用户名重复";
                    return Success(exist, msg);

                case 2:
                    exist = _userService.Query(c => c.Phone.Equals(data)).Result.Count();
                    return Success(exist, exist == 0 ? "校验成功" : "校验失败，手机号重复");

                default:
                    return Failed<int>("参数type不合法，校验未通过", 100);

            }

        }

        /// <summary>
        /// 根据用户名和密码查询用户
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [Route("query")]
        [HttpGet]
        public MessageModel<Users> QueryUser(string username, string password)
        {
            var user = _userService.Query(c => c.Username == username).Result.FirstOrDefault();
            if (user == null)
            {
                return Failed<Users>("查询的用户不存在！", 100);
            }

            if (MD5Helper.MD5EncodingWithSalt(password, user.Salt) != user.Password)
            {
                return Failed<Users>("密码错误", 100);
            }

            return Success(user);
        }
    }
}
