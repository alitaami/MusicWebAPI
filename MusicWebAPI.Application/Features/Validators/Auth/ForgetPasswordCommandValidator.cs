using FluentValidation;
using MusicWebAPI.Application.Features.Properties.Auth.Commands.ForgetPassword;

namespace MusicWebAPI.Application.Features.Validators.Auth
{
    public class ForgetPasswordCommandValidator : AbstractValidator<ForgetPasswordCommand>
    {
        public ForgetPasswordCommandValidator()
        {
            RuleFor(x => x.email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
        }
    }
}
