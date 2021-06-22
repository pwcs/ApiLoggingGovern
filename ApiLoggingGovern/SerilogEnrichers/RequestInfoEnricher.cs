using System;
using System.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace ApiLoggingGovern.SerilogEnrichers
{
    public class RequestInfoEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var httpContext = DependencyResolver.Current.GetService<IHttpContextAccessor>()?.HttpContext;
            if (null != httpContext)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestIP", httpContext.Request.Host.Host));

                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestPath", httpContext.Request.Path));

                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Referer", httpContext.Request.Headers["Referer"]));
            }

             
        }
    }

    public static class EnricherExtensions
    {
        public static LoggerConfiguration WithRequestInfo(this LoggerEnrichmentConfiguration enrich)
        {
            if (enrich == null)
                throw new ArgumentNullException(nameof(enrich));

            return enrich.With<RequestInfoEnricher>();
        }
    }
}
