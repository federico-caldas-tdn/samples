using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TeamDotNet.Api.Gateway
{
    /// <summary>
    /// Global Error Handling extension
    /// </summary>
    /// <seealso cref="Object" />
    public static class ErrorHandlingExt
    {
        /// <summary>
        /// Configures the exception handling.
        /// </summary>
        /// <param name="builder">
        /// The builder <see cref="IApplicationBuilder"/>
        /// </param>
        public static void ConfigureExceptionHandling(this IApplicationBuilder builder)
        {
            builder.UseExceptionHandler(appError => {
                appError.Run(async context => {

                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var exHandling = context.Features.Get<IExceptionHandlerFeature>();
                    ErrorDetails details = exHandling == null ?
                    new ErrorDetails
                    {
                        StatusCode = context.Response.StatusCode,
                        StackTrace = string.Empty,
                        Message = "Internal Server Error"
                    } :
                    new ErrorDetails
                    {
                        StatusCode = context.Response.StatusCode,
                        StackTrace = $"{exHandling.Error.StackTrace}",
                        Message = $"Error: {exHandling.Error.Message}"
                    };
                    await context.Response.WriteAsync(details.ToString());
                    Log.Error($"{details.Message} {details.StackTrace}");
                });
            });
        }

    }

}
