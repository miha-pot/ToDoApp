using FluentValidation;

namespace ToDoApp.Shared.AuthDTO.Token
{
    public class TokenRequestValidator : AbstractValidator<TokenRequest>
    {
        public TokenRequestValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token is required!");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Refresh token is required!");
        }
    }
}
