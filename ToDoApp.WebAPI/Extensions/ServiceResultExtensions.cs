using System.Net;
using ToDoApp.Shared.Common;

namespace ToDoApp.WebAPI.Extensions;

public static class ServiceResultExtensions
{
    public static IResult ToHttpResult<T>(this ServiceResult<T> result)
    {
        if (result.IsSuccess)
        {
            // If the service explicitly requested a 201 Created status, use it
            //if (result.StatusCode == HttpStatusCode.Created)
            //{
            //    //return Results.Json(result.Value, statusCode: StatusCodes.Status201Created);
            //    return Results.Ok(result.Value);
            //}

            return Results.Ok(result.Value);
        }

        return Results.Problem(title: result.ErrorTitle ?? "Operation Failed",
                               detail: result.ErrorDetail,
                               statusCode: (int)result.StatusCode);
    }
}
