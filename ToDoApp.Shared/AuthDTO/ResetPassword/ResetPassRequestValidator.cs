using FluentValidation;

namespace ToDoApp.Shared.AuthDTO.ResetPassword;

public class ResetPassRequestValidator : AbstractValidator<ResetPassRequest>
{
    public ResetPassRequestValidator()
    {
        RuleFor(x => x.Token)
           .NotEmpty().WithMessage("Token is required!");

        RuleFor(x => x.NewPassword)
           .NotEmpty().WithMessage("New password is required!");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required!")
            .Equal(x => x.NewPassword).WithMessage("Passwords are not equal!");
    }
}
