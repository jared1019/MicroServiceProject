using MicroService.Model;
using UserService.Interface.BASE;
using UserService.Model.Models;

namespace UserService.Interface
{
    public interface IUserServices : IBaseServices<SysUserModel>
    {
        Task<LoginUserDto> GetUser(string loginName, string loginPwd);
    }
}
