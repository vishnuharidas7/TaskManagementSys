using Org.BouncyCastle.Tsp;
using SendGrid.Helpers.Errors.Model;
using TaskManagementWebAPI.Common.ExceptionMessages;
using TaskManagementWebAPI.Domain.Exceptions;

namespace TaskManagementWebAPI.Middlewares
{
    public class ExceptionHandlingMiddleware
    {

        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }
      

       
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var (statusCode, message) = GetExceptionDetails(ex);
                await HandleExceptionAsync(context, statusCode, message, ex);
            }
        }

        private(int status,string messages) GetExceptionDetails(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, ExceptionMessages.ExceptionHandelingExceptions.UnauthorizedAccess),
                ArgumentNullException => (StatusCodes.Status400BadRequest, ExceptionMessages.ExceptionHandelingExceptions.NullValuesNotAllowed),
                HttpRequestException => (StatusCodes.Status503ServiceUnavailable, ExceptionMessages.ExceptionHandelingExceptions.ExternalServiceUnavailable),
                TaskCanceledException => (StatusCodes.Status504GatewayTimeout, ExceptionMessages.ExceptionHandelingExceptions.RequestTimedOut),
                InvalidOperationException => (StatusCodes.Status400BadRequest, ExceptionMessages.ExceptionHandelingExceptions.InvalidOperation),
                InvalidPhoneNumberException or
                InvalidEmailFormatException or
                InvalidRoleIdException or
                TaskValidationException or
                DuplicateEmailException or
                DuplicateUsernameException => (StatusCodes.Status400BadRequest, ex.Message),
                NotFoundException => (StatusCodes.Status404NotFound, ex.Message),
                TaskFileParserException => (StatusCodes.Status422UnprocessableEntity, ex.Message),
                AuthServiceUnavailableException => (StatusCodes.Status503ServiceUnavailable, ex.Message),
                TokenRefreshFailedException => (StatusCodes.Status504GatewayTimeout, ex.Message),
                _ => (StatusCodes.Status500InternalServerError, ExceptionMessages.ExceptionHandelingExceptions.UnexpectedError)
            };
        }
        private static async Task HandleExceptionAsync(HttpContext context, int statusCode, string message, Exception ex)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                StatusCode = statusCode,
                Message = message,
                Detail = ex.Message,
                Timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }

    }
}
