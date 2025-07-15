using Org.BouncyCastle.Tsp;
using SendGrid.Helpers.Errors.Model;
using TaskManagementWebAPI.CustomException;
using TaskManagementWebAPI.Domain.Custom_Exceptions;

namespace TaskManagementWebAPI.Middlewares
{
    public class ExceptionHandlingMiddleware
    {

        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next), "RequestDelegate cannot be null.");
        }
      

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (UnauthorizedAccessException ex)
            { 
                await HandleExceptionAsync(context, StatusCodes.Status401Unauthorized, "Unauthorized access", ex);
            }
            catch (ArgumentNullException ex)
            {
                await HandleExceptionAsync(context, StatusCodes.Status400BadRequest, "Null Values cannot be acceptable", ex);
            }
            catch (HttpRequestException ex)
            { 
                await HandleExceptionAsync(context, StatusCodes.Status503ServiceUnavailable, "External service unavailable", ex);
            }
            catch (TaskCanceledException ex)
            { 
                await HandleExceptionAsync(context, StatusCodes.Status504GatewayTimeout, "Request timed out", ex);
            }
            catch (InvalidOperationException ex)
            { 
                await HandleExceptionAsync(context, StatusCodes.Status400BadRequest, "Invalid operation", ex);
            }

            //Custom exception for phone number validation
            catch (InvalidPhoneNumberException ex)
            {
                await HandleExceptionAsync(context, StatusCodes.Status400BadRequest, ex.Message, ex);
            }

            //Custom exception for Email format validation
            catch (InvalidEmailFormatException ex)
            {
                await HandleExceptionAsync(context,StatusCodes.Status400BadRequest, ex.Message,ex);
            }

            //Custom exception for RoleId validation
            catch (InvalidRoleIdException ex)
            {
                await HandleExceptionAsync(context, StatusCodes.Status400BadRequest, ex.Message, ex); 
            } 

            catch (NotFoundException ex)
            {
                await HandleExceptionAsync(context, StatusCodes.Status404NotFound, ex.Message, ex);
            }
            //Custom exception
            catch (TaskValidationException ex)
            {
                await HandleExceptionAsync(context, StatusCodes.Status400BadRequest, ex.Message, ex);
            }
            //Custom exception
            catch (TaskFileParserException ex)
            {
                await HandleExceptionAsync(context, StatusCodes.Status422UnprocessableEntity, ex.Message, ex);
            }
            //Custom exception
            catch (AuthServiceUnavailableException ex)
            {
                await HandleExceptionAsync(context, StatusCodes.Status503ServiceUnavailable, ex.Message, ex);
            }
            //Custom exception
            catch (TokenRefreshFailedException ex)
            {
                await HandleExceptionAsync(context, StatusCodes.Status504GatewayTimeout, ex.Message, ex);
            }
            catch (Exception ex)
            { 
                await HandleExceptionAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred", ex);
            }
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
