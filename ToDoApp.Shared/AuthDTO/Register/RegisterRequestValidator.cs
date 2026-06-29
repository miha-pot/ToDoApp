using FluentValidation;

namespace ToDoApp.Shared.AuthDTO.Register;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required!")
            .EmailAddress().WithMessage("A valid email is required!");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required!")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required!")
            .Equal(x => x.Password).WithMessage("Confirmed password must be equal to set password!");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required!");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required!");
    }
}
