using Grpc.Core;
using MicroService.Common;
using UserService.Interface;

namespace User.GrpcService.Services
{
    public class UsersService : Users.UsersBase
    {
        private readonly ILogger<UsersService> _logger;
        private readonly IUserServices _userService;
        public UsersService(ILogger<UsersService> logger, IUserServices userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public override Task<UsersResponse> QueryUser(AuthData request, ServerCallContext context)
        {
            UsersResponse result = new UsersResponse();

            var user = _userService.Query(c => c.Username == request.Username).Result.FirstOrDefault();
            if (user == null)
            {
                result.Success = false;
                result.ErrorMsg = "查询的用户不存在";
                return Task.FromResult(result);
            }

            if (MD5Helper.MD5EncodingWithSalt(request.Password, user.Salt) != user.Password)
            {
                result.Success = false;
                result.ErrorMsg = "密码错误";
                return Task.FromResult(result);
            }

            result.Success = true;
            result.Data = new UsersItemResponse()
            {
                Id = user.Id,
                Username = user.Username,
                Phone = user.Phone,
                Salt = user.Salt
            };
            return Task.FromResult(result);
        }
    }
}
