using FluentValidation;
using MusicWebAPI.Application.Features.Properties.Subscription.Commands.Subscribe;

namespace MusicWebAPI.Application.Features.Validators.Subscription
{
    public class SubscribeCommandValidator : AbstractValidator<SubscribeCommand>
    {
        public SubscribeCommandValidator()
        {
            RuleFor(x => x.PlanId)
                .NotEmpty().WithMessage("PlanId is required.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");
        }
    }
}
