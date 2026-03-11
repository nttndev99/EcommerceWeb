using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ecommerce.MVC.Filters;

/// <summary>
/// Validates ModelState before action executes.
/// Returns BadRequest with validation errors automatically.
/// </summary>
public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            // For Ajax/API calls return JSON
            if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                context.Result = new BadRequestObjectResult(new { errors });
            }
            else
            {
                // For normal form posts, let the controller handle it
                // But still call base
                base.OnActionExecuting(context);
            }
        }
    }
}

/// <summary>
/// Logs action execution time for performance monitoring.
/// </summary>
public class PerformanceLogAttribute : ActionFilterAttribute
{
    private System.Diagnostics.Stopwatch? _stopwatch;
    private readonly ILogger<PerformanceLogAttribute> _logger;

    public PerformanceLogAttribute(ILogger<PerformanceLogAttribute> logger)
    {
        _logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        _stopwatch = System.Diagnostics.Stopwatch.StartNew();
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        _stopwatch?.Stop();
        var elapsed = _stopwatch?.ElapsedMilliseconds ?? 0;

        if (elapsed > 500)
        {
            _logger.LogWarning(
                "Slow action detected: {Controller}/{Action} took {Ms}ms",
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"],
                elapsed);
        }
    }
}

/// <summary>
/// Global exception filter for centralized error handling.
/// </summary>
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception,
            "Unhandled exception in {Controller}/{Action}",
            context.RouteData.Values["controller"],
            context.RouteData.Values["action"]);

        if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            context.Result = new JsonResult(new { error = "An unexpected error occurred." })
            {
                StatusCode = 500
            };
            context.ExceptionHandled = true;
        }
    }
}

/// <summary>
/// Result action filter - adds common ViewData to all views.
/// </summary>
public class AdminLayoutFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Controller is Controller controller)
        {
            controller.ViewData["AppName"] = "Ecommerce Admin";
        }
    }
}