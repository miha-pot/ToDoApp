using FluentValidation;

namespace ToDoApp.Shared.AuthDTO.ForgotPassword;

public class ForgotPassRequestValidator : AbstractValidator<ForgotPassRequest>
{
    public ForgotPassRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required!")
            .EmailAddress().WithMessage("A valid email is required!");

        RuleFor(x => x.ClientUri)
           .NotEmpty().WithMessage("Client Uri is required!");
    }
}
