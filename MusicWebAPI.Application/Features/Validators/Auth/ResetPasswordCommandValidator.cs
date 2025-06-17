using FluentValidation;
using MusicWebAPI.Application.Features.Properties.Auth.Commands.ResetPassword;
using System.Web.WebPages;

namespace MusicWebAPI.Application.Features.Validators.Auth
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.newPassword)
               .NotEmpty().WithMessage("Password is required.")
               .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
               .Matches(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
                   .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");

            RuleFor(x => x.otpCode)
                .NotEmpty().WithMessage("Otp Code is required.")
                .Must(x => x.Is<int>() && x.Length == 6).WithMessage("OTP should be a 6 digit number!");
        }
    }
}
