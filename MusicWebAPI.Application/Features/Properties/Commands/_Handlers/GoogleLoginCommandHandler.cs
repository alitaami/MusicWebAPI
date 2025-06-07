using MediatR;
using MusicWebAPI.Application.Commands;
using MusicWebAPI.Application.Features.Properties.Commands.Auth;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Application.ViewModels.UserViewModel;

namespace MusicWebAPI.Application.Features.Properties.Commands.Handlers
{
    public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, string>
    {
        private readonly IServiceManager _serviceManager;

        public GoogleLoginCommandHandler(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task<string> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
        {
            // getting the JWT token by user properties
            var token = await _serviceManager.User.GoogleLogin(request.IdToken);

            return token;
        }
    }
}
