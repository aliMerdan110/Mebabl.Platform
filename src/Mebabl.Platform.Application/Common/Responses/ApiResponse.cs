namespace Mebabl.Platform.Application.Common.Responses;

public class ApiResponse<T>
{
    public bool Success { get; init; }

    public string? Message { get; init; }

    public T? Data { get; init; }

    public IEnumerable<string>? Errors { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null)
        => new()
        {
            Success = true,
            Data = data,
            Message = message
        };

    public static ApiResponse<T> Fail(
        IEnumerable<string> errors,
        string? message = null)
        => new()
        {
            Success = false,
            Errors = errors,
            Message = message
        };
}