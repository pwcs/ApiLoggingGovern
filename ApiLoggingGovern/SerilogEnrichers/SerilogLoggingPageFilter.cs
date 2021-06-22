using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ApiLoggingGovern.SerilogEnrichers
{
    public class SerilogLoggingPageFilter : IPageFilter
    {
        private readonly IDiagnosticContext _diagnosticContext;

        public SerilogLoggingPageFilter(IDiagnosticContext diagnosticContext)
        {
            _diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));
        }
        //Required by the interface
        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
        }
        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
        }
        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
            var name = context.HandlerMethod?.Name ?? context.HandlerMethod?.MethodInfo.Name;
            if (name != null)
            {
                _diagnosticContext.Set("RazorPageHandler", name);
            }
        }
    }
}
