using FluentValidation;

namespace ToDoApp.Shared.Extensions;

public static class FluentValidationExtensions
{
    public static Func<object, string, Task<IEnumerable<string>>> ValidateValue<T>(this IValidator<T> validator)
    {
        return async (model, propertyName) =>
        {
            if (model is not T typedModel)
                return [];

            var result = await validator.ValidateAsync(
                ValidationContext<T>.CreateWithOptions(typedModel, x => x.IncludeProperties(propertyName))
            );

            if (result.IsValid)
                return [];

            return result.Errors.Select(e => e.ErrorMessage);
        };
    }
}