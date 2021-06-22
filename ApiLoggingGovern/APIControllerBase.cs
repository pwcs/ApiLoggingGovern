using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace ApiLoggingGovern
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected readonly ILogger Logger;
        protected ApiControllerBase(ILogger logger)
        {
            Logger = logger;
        }
    }
}
