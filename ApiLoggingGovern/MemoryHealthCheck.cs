using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApiLoggingGovern
{
    public class MemoryHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            //Log.Debug("MemoryHealthCheck");
            //doing some memory check things.
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
