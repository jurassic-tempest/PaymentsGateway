using Microsoft.Net.Http.Headers;

using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;

using System.Diagnostics;
using System.Net;

namespace PaymentGateway.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly RequestDelegate _next;

        private const string DefaultErrorMessage = "Sorry, something went wrong!";

        public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger, RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BankException ex)
            {
                await ErrorsHelper.SetErrorResponseAsync(context,
                    message: ex.Message,
                    errorCode: ex.Code,
                    statusCode: ex.StatusCode,
                    customErrors: ex.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception was thrown by the application");
            }
        }
    }
}
