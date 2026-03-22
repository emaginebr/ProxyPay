using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;

namespace Ganesha.API.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is UnauthorizedAccessException)
            {
                context.Result = new ForbidResult();
                context.ExceptionHandled = true;
                return;
            }

            _logger.LogError(context.Exception, "Unhandled exception on {Method} {Path}",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path);

            context.Result = new ObjectResult(context.Exception.Message)
            {
                StatusCode = 500
            };
            context.ExceptionHandled = true;
        }
    }
}
