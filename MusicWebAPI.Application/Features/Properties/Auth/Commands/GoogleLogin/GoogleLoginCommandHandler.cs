using MediatR;
using MusicWebAPI.Domain.Interfaces.Services.Base;

namespace MusicWebAPI.Application.Features.Properties.Auth.Commands.GoogleLogin
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
