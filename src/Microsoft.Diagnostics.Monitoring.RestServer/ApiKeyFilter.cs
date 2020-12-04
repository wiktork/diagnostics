using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.RestServer
{
    internal sealed class ApiKeyFilter : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeader = "X-API-KEY";

        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            IConfiguration configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var authOptions = new AuthOptions();
            configuration.Bind(authOptions);

            if (authOptions.AuthRequired)
            {
                if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeader, out StringValues values))
                {
                    context.Result = new JsonResult(new ProblemDetails { Status = 401, Detail = $"No {ApiKeyHeader} specified" }) { StatusCode = 401 };
                    return Task.CompletedTask;
                }

                if (string.IsNullOrEmpty(authOptions.ApiKey))
                {
                    context.Result = new JsonResult(new ProblemDetails { Status = 401, Detail = $"No API Key configured" }) { StatusCode = 401 };
                }
                
                if (!string.Equals(authOptions.ApiKey, values.FirstOrDefault()))
                {
                    context.Result = new JsonResult(new ProblemDetails { Status = 401, Detail = $"Invalid API Key" }) { StatusCode = 401 };
                    return Task.CompletedTask;
                }
            }

            return next();
        }
    }
}
