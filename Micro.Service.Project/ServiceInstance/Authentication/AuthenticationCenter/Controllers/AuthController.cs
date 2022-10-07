using Grpc.Net.Client;
using MicroService.Framework.JWTExtend;
using MicroService.Model;
using Microsoft.AspNetCore.Mvc;
using User.GrpcService;

namespace AuthenticationCenter.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    public class AuthController : BaseApiController
    {
        [HttpGet]
        [Route("Accredit")]
        public MessageModel<string> Accredit([FromServices] IJWTService _jwtService, string username = "admin", string password = "123456")
        {
            try
            {
                using (var channel = GrpcChannel.ForAddress("http://localhost:5238"))
                {
                    var client = new Users.UsersClient(channel);
                    var reply = client.QueryUser(new AuthData() { Password = password, Username = username });
                    if (reply.Success && reply.Data != null)
                    {
                        string token = _jwtService.GetToken(new JWTUserModel()
                        {
                            Id = reply.Data.Id,
                            UserName = reply.Data.Username,
                            Phone = reply.Data.Phone
                        });
                        return Success<string>(token);
                    }
                    else
                    {
                        return Failed<string>(reply.ErrorMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                return Failed<string>("获取授权异常，请稍候再试");
            }
        }
    }
}
