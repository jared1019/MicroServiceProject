using DotNetCore.CAP;
using MicroService.Model;

namespace MicroService.Service
{
    public class CapUserService : ICapSubscribe
    {
        [CapSubscribe("UserService")]
        public async Task Handler(IEnumerable<User> userList)
        {

        }
    }
}
