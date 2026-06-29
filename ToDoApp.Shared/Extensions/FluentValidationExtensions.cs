using FluentValidation;

namespace ToDoApp.Shared.Extensions;

public static class FluentValidationExtensions
{
    /// <summary>
    /// Generira delegat za validacijo posamezne lastnosti modela, ki ga zahteva MudBlazor.
    /// </summary>
    public static Func<object, string, Task<IEnumerable<string>>> ValidateValue<T>(this IValidator<T> validator)
    {
        return async (model, propertyName) =>
        {
            // Če model ni pravilnega tipa, vrnemo prazno (varnostni mehanizem)
            if (model is not T typedModel)
                return [];

            // Validiramo samo specifično polje (propertyName), ki ga je javil MudBlazor field
            var result = await validator.ValidateAsync(
                ValidationContext<T>.CreateWithOptions(typedModel, x => x.IncludeProperties(propertyName))
            );

            if (result.IsValid)
                return [];

            return result.Errors.Select(e => e.ErrorMessage);
        };
    }
}