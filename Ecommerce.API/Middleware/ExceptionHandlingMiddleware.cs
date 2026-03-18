using System.Net;
using System.Text.Json;

namespace Ecommerce.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate  _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(ctx, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        ctx.Response.ContentType = "application/json";

        var (status, message) = ex switch
        {
            KeyNotFoundException   => (HttpStatusCode.NotFound,            ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized,   ex.Message),
            ArgumentException      => (HttpStatusCode.BadRequest,          ex.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest,       ex.Message),
            _                      => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        ctx.Response.StatusCode = (int)status;

        var response = JsonSerializer.Serialize(new
        {
            success = false,
            message,
        });

        return ctx.Response.WriteAsync(response);
    }
}