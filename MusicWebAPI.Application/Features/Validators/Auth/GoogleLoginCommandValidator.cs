using FluentValidation;
using MusicWebAPI.Application.Commands;
using MusicWebAPI.Application.Features.Properties.Commands.Auth;

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
