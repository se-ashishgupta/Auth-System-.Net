using System.Net;
using System.Text.Json;

namespace AuthSystem.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);  // ← pass request to next middleware
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception caught by middleware");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        context.Response.StatusCode = ex switch
        {
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,  // 401
            KeyNotFoundException        => (int)HttpStatusCode.NotFound,       // 404
            ArgumentException           => (int)HttpStatusCode.BadRequest,     // 400
            _                           => (int)HttpStatusCode.InternalServerError // 500
        };

        var result = JsonSerializer.Serialize(new
        {
            statusCode = context.Response.StatusCode,
            message = ex.Message
        });

        return context.Response.WriteAsync(result);
    }
}