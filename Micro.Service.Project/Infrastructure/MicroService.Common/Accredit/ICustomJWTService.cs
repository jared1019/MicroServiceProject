using MicroService.Model;

namespace MicroService.Common
{
    public interface ICustomJWTService
    {
        string GetToken(string UserName, string password, LoginUserDto user);
    }
}
