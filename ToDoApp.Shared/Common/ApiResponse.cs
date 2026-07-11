namespace ToDoApp.Shared.Common;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public string? ErrorTitle { get; set; }
    public string? ErrorDetail { get; set; }

    public bool IsWarning { get; set; }
    public string? WarningTitle { get; set; }
    public string? WarningDetail { get; set; }

    public int StatusCode { get; set; }
    public Dictionary<string, string[]>? Errors { get; init; }
    public bool IsValidationError =>
     !IsSuccess && Errors is { Count: > 0 };

    public static ApiResponse<T> Success(T value, int statusCode) =>
        new() { IsSuccess = true, Value = value, StatusCode = statusCode };

    public static ApiResponse<T> Success(T value) =>
        new() { IsSuccess = true, Value = value, StatusCode = 200 };

    public static ApiResponse<T> Failure(string? title,
                                         string? detail,
                                         int statusCode,
                                         Dictionary<string, string[]>? errors = null) =>
        new() { IsSuccess = false, ErrorTitle = title, ErrorDetail = detail, StatusCode = statusCode, Errors = errors };

    public static ApiResponse<T> Empty() => new()
    {
        IsSuccess = true,
        StatusCode = 0
    };
}
