using System.Diagnostics;
using System.Net;

using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Middleware
{
    public static class ErrorsHelper
    {
        private const string DefaultErrorMessage = "Sorry, something went wrong!";

        public static Task SetErrorResponseAsync(HttpContext context,
                string? message = null,
                string? errorCode = null,
                HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
                List<Error> customErrors = null)
        {
            var errors = new ErrorsResponse();

            if (customErrors?.Count > 0)
            {
                foreach (var error in customErrors)
                {
                    errors.Errors.Add(new Error
                    {
                        Code = error.Code,
                        Message = error.Message
                    });
                }
            }
            else
            {
                string errorMessage = message ?? DefaultErrorMessage;
                errors.Errors.Add(new Error
                {
                    Code = errorCode ?? statusCode.ToString(),
                    Message = errorMessage
                });
            }

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";

            var jsonErrors = errors.Serialize();
            return context.Response.WriteAsync(jsonErrors);
        }
    }
}
