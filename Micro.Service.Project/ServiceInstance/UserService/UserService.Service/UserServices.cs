using MicroService.Model;
using UserService.Interface;
using UserService.Model.Models;
using UserService.Service.BASE;

namespace UserService.Service
{
    public class UserServices : BaseServices<Users>, IUserServices
    {
        public async Task<LoginUserDto> GetUser(string loginName, string loginPwd)
        {
            LoginUserDto loginUserDto = new LoginUserDto()
            {
                UserId = 123,
                LoginName = loginName,
                RealName = "admin",
                RoleName = "超级系统管理员",
            };
            var user = (await Query(a => a.Username == loginName && a.Password == loginPwd)).FirstOrDefault();
            Task.WaitAll();
            //if (user != null)
            //{
            //    loginUserDto=new LoginUserDto() { 

            //    };
            //}
            return loginUserDto;
        }
    }
}
