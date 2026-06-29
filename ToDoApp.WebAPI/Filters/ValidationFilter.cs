using FluentValidation;

namespace ToDoApp.WebAPI.Filters;

public class ValidationFilter<T> : IEndpointFilter
    where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
                                                EndpointFilterDelegate next)
    {
        if (context.Arguments.FirstOrDefault(x => x is T) is not T dto)
        {
            return Results.Problem(
                title: "Invalid Request Body",
                detail: "A valid request body is required. The sent data is either empty or structurally malformed.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is not null)
        {
            var validationResult = await validator.ValidateAsync(dto, context.HttpContext.RequestAborted);
            if (!validationResult.IsValid)
            {
                var errorDictionary = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                return Results.ValidationProblem(
                    errors: errorDictionary,
                    detail: "Please fix the highlighted fields before submitting again.",
                    title: "Validation Error"
                );
            }
        }

        return await next(context);
    }
}
