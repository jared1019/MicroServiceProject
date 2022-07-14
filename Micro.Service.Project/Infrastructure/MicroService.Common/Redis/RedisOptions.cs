namespace MicroService.Common
{
    public class RedisOptions
    {
        public string Host { get; set; }
        public int DB { get; set; } = 0;
        public int Prot { get; set; }
        public string Password { get; set; }
    }
}
