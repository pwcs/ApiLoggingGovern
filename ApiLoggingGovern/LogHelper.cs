using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiLoggingGovern
{
    public static class LogHelper
    {
        public static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
        {
            var request = httpContext.Request;

            // Set all the common properties available for every request
            diagnosticContext.Set("Host", request.Host);
            diagnosticContext.Set("Protocol", request.Protocol);
            diagnosticContext.Set("Scheme", request.Scheme);

            // Only set it if available. You're not sending sensitive data in a querystring right?!
            if (request.QueryString.HasValue)
            {
                diagnosticContext.Set("QueryString", request.QueryString.Value);
            }

            // Set the content-type of the Response at this point
            diagnosticContext.Set("ContentType", httpContext.Response.ContentType);

            // Retrieve the IEndpointFeature selected for the request
            var endpoint = httpContext.GetEndpoint();
            if (endpoint is object) // endpoint != null
            {
                diagnosticContext.Set("EndpointName", endpoint.DisplayName);
            }
        }

        //假设您希望将摘要日志记录为Debug而不是Information。首先，您将创建一个具有以下所需逻辑的辅助函数，如下所示：
        //然后，您可以在调用时设置级别功能UseSerilogRequestLogging()：
        public static LogEventLevel CustomGetLevel(HttpContext ctx, double _, Exception ex) =>
        ex != null
            ? LogEventLevel.Error
            : ctx.Response.StatusCode > 499
                ? LogEventLevel.Error
                : ctx.Response.StatusCode == 200 ? LogEventLevel.Information : LogEventLevel.Debug; //Debug instead of Information

        public static LogEventLevel ExcludeHealthChecks(HttpContext ctx, double _, Exception ex) =>
       ex != null
           ? LogEventLevel.Error
           : ctx.Response.StatusCode > 499
               ? LogEventLevel.Error
               : IsHealthCheckEndpoint(ctx) // Not an error, check if it was a health check
                   ? LogEventLevel.Verbose // Was a health check, use Verbose
                   : LogEventLevel.Information;


        private static bool IsHealthCheckEndpoint(HttpContext ctx)
        {
            var endpoint = ctx.GetEndpoint();
            if (endpoint is object) // same as !(endpoint is null)
            {
                return string.Equals(
                    endpoint.DisplayName,
                    "Health checks",
                    StringComparison.Ordinal);
            }
            // No endpoint, so not a health check endpoint
            return false;
        }







        /// <summary>
        /// Create a <see cref="Serilog.AspNetCore.RequestLoggingOptions.GetLevel"> method that
        /// uses the default logging level, except for the specified endpoint names, which are
        /// logged using the provided <paramref name="traceLevel" />.
        /// </summary>
        /// <param name="traceLevel">The level to use for logging "trace" endpoints</param>
        /// <param name="traceEndointNames">The display name of endpoints to be considered "trace" endpoints</param>
        /// <returns></returns>
        public static Func<HttpContext, double, Exception, LogEventLevel> GetLevel(LogEventLevel traceLevel, params string[] traceEndointNames)
        {
            if (traceEndointNames is null || traceEndointNames.Length == 0)
            {
                throw new ArgumentNullException(nameof(traceEndointNames));
            }

            return (ctx, _, ex) =>
                IsError(ctx, ex)
                ? LogEventLevel.Error
                : IsTraceEndpoint(ctx, traceEndointNames)
                    ? traceLevel
                    : LogEventLevel.Information;
        }

        static bool IsError(HttpContext ctx, Exception ex)
            => ex != null || ctx.Response.StatusCode > 499;

        static bool IsTraceEndpoint(HttpContext ctx, string[] traceEndoints)
        {
            var endpoint = ctx.GetEndpoint();
            if (endpoint is object)
            {
                for (var i = 0; i < traceEndoints.Length; i++)
                {
                    if (string.Equals(traceEndoints[i], endpoint.DisplayName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
