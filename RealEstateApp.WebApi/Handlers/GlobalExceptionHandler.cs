using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Exceptions;

namespace RealEstateApp.WebApi.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
            Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Type = "https://datatracker.ietf.org/doc/html/rfc7807"
            };

            switch (exception)
            {
                case ValidationException validationException:
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Title = "Validation Error";
                    problemDetails.Detail = string.Join(", ", validationException.Errors);
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;

                case ApiException apiException:
                    problemDetails.Status = apiException.StatusCode;
                    problemDetails.Title = "API Error";
                    problemDetails.Detail = apiException.Message;
                    httpContext.Response.StatusCode = apiException.StatusCode;
                    break;

                default:
                    problemDetails.Status = StatusCodes.Status500InternalServerError;
                    problemDetails.Title = "Server Error";
                    problemDetails.Detail = exception.Message;
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}