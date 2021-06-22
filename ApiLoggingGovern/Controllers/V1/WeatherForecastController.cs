using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApiLoggingGovern.Controllers.V1
{
 
    [ApiController]
    [Route("api/"+ApiConfig.V1+"/[controller]")]
    [ApiExplorerSettings(GroupName = ApiConfig.V1 )]
    public class WeatherForecastController : ControllerBase
    {
        static int _callCount;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        readonly IDiagnosticContext _diagnosticContext;
        public WeatherForecastController(ILogger<WeatherForecastController> logger, IDiagnosticContext diagnosticContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));
        }

        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
           
            Log.Information("测试1"); // <-添加此行
         


            Log.Information(":\n{@LoginData}",
                new LoginData { Username = "aa", Password = "bbb" });



            _diagnosticContext.Set("IndexCallCount", Interlocked.Increment(ref _callCount));
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }


        

    }
}
