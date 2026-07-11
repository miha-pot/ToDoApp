using System.Net;
using ToDoApp.Shared.Common;

namespace ToDoApp.WebAPI.Extensions;

public static class ServiceResultExtensions
{
    public static IResult ToHttpResult<T>(this ServiceResult<T> result)
    {
        if (!result.IsSuccess)
        {
            return Results.Problem(title: result.ErrorTitle ?? "Operation Failed",
                             detail: result.ErrorDetail,
                             statusCode: (int)result.StatusCode);
        }

        return Results.Json(result, statusCode: StatusCodes.Status200OK);
    }
}
