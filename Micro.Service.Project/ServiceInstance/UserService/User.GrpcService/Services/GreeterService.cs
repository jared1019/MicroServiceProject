using Grpc.Core;
using UserService.Interface;

namespace User.GrpcService.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        private readonly IUserServices _userService;
        public GreeterService(ILogger<GreeterService> logger, IUserServices userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
    }
}