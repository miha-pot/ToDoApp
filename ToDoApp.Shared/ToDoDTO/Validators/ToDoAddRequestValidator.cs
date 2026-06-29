using FluentValidation;
using ToDoApp.Shared.ToDoDTO.Commands;

namespace ToDoApp.Shared.ToDoDTO.Validators;

public class ToDoAddRequestValidator : AbstractValidator<ToDoAddRequest>
{
    public ToDoAddRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required!");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Length must be less than 500 characters!");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Due date cannot be in the past.")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.Level)
            .IsInEnum().WithMessage("Value must be between 1 and 5");
    }
}
