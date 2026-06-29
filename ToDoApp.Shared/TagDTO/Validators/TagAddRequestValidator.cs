using FluentValidation;
using ToDoApp.Shared.TagDTO.Commands;

namespace ToDoApp.Shared.TagDTO.Validators;

public class TagAddRequestValidator : AbstractValidator<TagAddRequest>
{
    public TagAddRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name is required!");

        RuleFor(x => x.ColorHex)
            .NotEmpty().WithMessage("Tag color is required!");
    }
}
