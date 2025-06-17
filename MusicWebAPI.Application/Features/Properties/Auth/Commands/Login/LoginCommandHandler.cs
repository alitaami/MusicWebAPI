using MediatR;
using Microsoft.Extensions.Configuration;
using MusicWebAPI.Domain.Interfaces.Services;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Application.ViewModels.UserViewModel;

namespace MusicWebAPI.Application.Features.Properties.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginUserViewModel>
    {
        private readonly IServiceManager _serviceManager;

        public LoginCommandHandler(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task<LoginUserViewModel> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // getting the JWT token by user properties
             var token = await _serviceManager.User.LoginUser(request.Email, request.Password);

            return new LoginUserViewModel
            {
                Token = token
            };
        }
    }
}
