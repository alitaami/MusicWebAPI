using FluentValidation;
using MusicWebAPI.Application.Commands;
using MusicWebAPI.Application.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.Features.Validators.NewFolder
{
   public class Base
    {
        public class CreatePropertyRequestValidator : AbstractValidator<RegisterUserCommand>
        {
            public CreatePropertyRequestValidator()
            {
                //RuleFor(x => x.UserName)
                //    .SetValidator(new RegisterUserCommandValidator());
            }
        }
    }
}
