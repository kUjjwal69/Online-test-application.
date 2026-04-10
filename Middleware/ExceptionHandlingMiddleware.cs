using System.Net;
using System.Text.Json;
using TestManagementApplication.Common;

namespace TestManagementApplication.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt.");
                await WriteErrorResponse(context, HttpStatusCode.Unauthorized, "Unauthorized access.", ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found.");
                await WriteErrorResponse(context, HttpStatusCode.NotFound, "Resource not found.", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation.");
                await WriteErrorResponse(context, HttpStatusCode.BadRequest, "Invalid operation.", ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument error.");
                await WriteErrorResponse(context, HttpStatusCode.BadRequest, "Bad request.", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await WriteErrorResponse(context, HttpStatusCode.InternalServerError,
                    "An internal server error occurred.", "Please contact support.");
            }
        }

        private static async Task WriteErrorResponse(
            HttpContext context, HttpStatusCode statusCode, string message, string detail)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse.Fail(message, new List<string> { detail });
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
