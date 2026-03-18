using Microsoft.AspNetCore.Mvc;
using Ecommerce.API.Models;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected string? CurrentUserId =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;

    protected IActionResult Ok<T>(T data, string? message = null)
        => base.Ok(ApiResponse<T>.Ok(data, message));

    protected IActionResult Created<T>(string location, T data, string? message = null)
        => base.Created(location, ApiResponse<T>.Ok(data, message));

    protected IActionResult BadRequest(string message, object? errors = null)
        => base.BadRequest(ApiResponse.Fail(message, errors));

    protected IActionResult NotFound(string message)
        => base.NotFound(ApiResponse.Fail(message));

    protected IActionResult Unauthorized(string message = "Unauthorized.")
        => base.Unauthorized(ApiResponse.Fail(message));

    protected IActionResult Forbidden(string message = "Forbidden.")
        => StatusCode(403, ApiResponse.Fail(message));

    protected IActionResult ServerError(string message = "Internal server error.")
        => StatusCode(500, ApiResponse.Fail(message));

    protected IActionResult FromResult<T>(
        Ecommerce.Application.Common.Result<T> result,
        string? successMessage = null)
    {
        return result.IsSuccess
            ? Ok(result.Data!, successMessage)
            : BadRequest(result.Error!);
    }

    protected IActionResult FromResult(
        Ecommerce.Application.Common.Result result,
        string? successMessage = null)
    {
        return result.IsSuccess
            ? base.Ok(ApiResponse.Ok(successMessage))
            : BadRequest(result.Error!);
    }

    protected IActionResult Paged<T>(
        Ecommerce.Application.Common.PagedResult<T> result,
        string? message = null)
        => base.Ok(PagedApiResponse<T>.Ok(
            result.Items, result.TotalCount, result.PageNumber, result.PageSize, message));
}