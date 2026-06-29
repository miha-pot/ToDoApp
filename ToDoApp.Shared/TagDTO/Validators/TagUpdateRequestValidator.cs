using FluentValidation;
using ToDoApp.Shared.TagDTO.Commands;

namespace ToDoApp.Shared.TagDTO.Validators;

public class TagUpdateRequestValidator : AbstractValidator<TagUpdateRequest>
{
    public TagUpdateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name is required!");

        RuleFor(x => x.ColorHex)
            .NotEmpty().WithMessage("Tag color is required!");
    }
}
