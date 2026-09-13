using FluentValidation;
using ToDoApp.Shared.ToDoDTO.Commands;

namespace ToDoApp.Shared.ToDoDTO.Validators;

public class ToDoUpdateRequestValidator : AbstractValidator<ToDoUpdateRequest>
{
    public ToDoUpdateRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required!");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required!");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required!");

        RuleFor(x => x.Level)
            .IsInEnum().WithMessage("Invalid priority level selected. Must be between 1 and 5.");
    }
}
