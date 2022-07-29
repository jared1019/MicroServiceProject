using Microsoft.AspNetCore.Mvc;
using UserService.Interface;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("/api/user/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IUserServices _userServices;

        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IUserServices userServices)
        {
            _logger = logger;
            this._userServices = userServices;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            _userServices.GetUser("1", "22");
            _userServices.QueryById(123);

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}