using FluentValidation;
using MusicWebAPI.Application.Features.Properties.Auth.Commands.Login;

namespace MusicWebAPI.Application.Features.Validators.Auth
{
    public class LoginUserCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
