using System.Net;

namespace ToDoApp.Shared.Common;

public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public string? ErrorTitle { get; set; }
    public string? ErrorDetail { get; set; }

    public HttpStatusCode StatusCode { get; set; }

    public static ServiceResult<T> Success(T value, HttpStatusCode statusCode = HttpStatusCode.Created) => new()
    {
        IsSuccess = true,
        Value = value,
        StatusCode = statusCode
    };

    public static ServiceResult<T> Failure(string title,
                                           string detail,
                                           HttpStatusCode statusCode)
        => new()
        {
            IsSuccess = false,
            ErrorTitle = title,
            ErrorDetail = detail,
            StatusCode = statusCode
        };

    public static ServiceResult<T> Failure(string title,
                                           string detail)
        => new()
        {
            IsSuccess = false,
            ErrorTitle = title,
            ErrorDetail = detail
        };
}

