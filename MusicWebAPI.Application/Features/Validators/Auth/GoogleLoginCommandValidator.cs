using FluentValidation;
using MusicWebAPI.Application.Features.Properties.Auth.Commands.GoogleLogin;

namespace MusicWebAPI.Application.Features.Validators.Auth
{
    public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
    {
        public GoogleLoginCommandValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage("IdToken is required.");
        }
    }
}
