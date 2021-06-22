using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiLoggingGovern.Controllers.V2
{

    /// <summary>
    /// V22222
    /// </summary>
    
    [ApiController]
    [Route("api/" + ApiConfig.V2 + "/[controller]")]
    [ApiExplorerSettings(GroupName = ApiConfig.V2)]
    public class WeatherForecastController : ApiControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };


        public WeatherForecastController(ILogger<WeatherForecastController> logger) : base(logger)
        {
        }

        /// <summary>
        /// V1在在
        /// </summary>
        /// <returns></returns>
        [HttpGet] 
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            //_logger.LogInformation("1111111");
            //_logger.LogError("22222222222222");
            //Logger.LogWarning("aaaaaaaaaaaaa");
            Log.Error("Test Exception as an example");
            Log.ForContext<Program>().Information("a");
            string a = "a";
            try {
                Convert.ToInt32(a);
            }
            catch(Exception e)
            {
                Log.Error("1、"+e.Message);
                throw new Exception("2、"+e.Message);
            }
         //throw new Exception("Test Exception as an example");
            
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
