namespace Ecommerce.API.Models;

// ─────────────────────────────────────────────
// RESPONSE WRAPPER
// ─────────────────────────────────────────────
public class ApiResponse<T>
{
    public bool    Success { get; set; }
    public string? Message { get; set; }
    public T?      Data    { get; set; }
    public object? Errors  { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Message = message,
        Data    = data,
    };

    public static ApiResponse<T> Fail(string message, object? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors  = errors,
    };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string? message = null) => new()
    {
        Success = true,
        Message = message,
    };

    public new static ApiResponse Fail(string message, object? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors  = errors,
    };
}

// ─────────────────────────────────────────────
// PAGED RESPONSE
// ─────────────────────────────────────────────
public class PagedApiResponse<T>
{
    public bool         Success    { get; set; }
    public string?      Message    { get; set; }
    public IEnumerable<T> Data    { get; set; } = Enumerable.Empty<T>();
    public PaginationMeta Pagination { get; set; } = new();

    public static PagedApiResponse<T> Ok(
        IEnumerable<T> data, int totalCount, int pageNumber, int pageSize,
        string? message = null) => new()
    {
        Success = true,
        Message = message,
        Data    = data,
        Pagination = new PaginationMeta
        {
            TotalCount   = totalCount,
            PageNumber   = pageNumber,
            PageSize     = pageSize,
            TotalPages   = (int)Math.Ceiling((double)totalCount / pageSize),
            HasPrevious  = pageNumber > 1,
            HasNext      = pageNumber < (int)Math.Ceiling((double)totalCount / pageSize),
        },
    };
}

public class PaginationMeta
{
    public int  TotalCount  { get; set; }
    public int  PageNumber  { get; set; }
    public int  PageSize    { get; set; }
    public int  TotalPages  { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext     { get; set; }
}