namespace MicroService.Common
{
    public class ConfigInformation
    {
        public string RootUrl { get; set; }

        public string UserUrl { get; set; }

        public JWTTokenOptions JWTTokenOptions { get; set; }
    }


    public class JWTTokenOptions
    {
        public string Audience { get; set; }

        public string SecurityKey { get; set; }

        public string Issuer { get; set; }
    }
}
