using MediatR;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using MusicWebAPI.Application.Commands;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System;

namespace MusicWebAPI.Application.Commands.Handlers
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, string>
    {
        private readonly IServiceManager _serviceManager;

        public RegisterUserHandler(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task<string> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // Call the UserService to register the user
            var registeredUser = await _serviceManager.User.RegisterUser(new User
            {
                UserName = request.UserName,
                Email = request.Email,
                FullName = request.FullName,
                IsArtist = request.IsArtist
            }, request.Password);

            // Return the User Id (or any other information) after registration
            return registeredUser.Id;  
        }
    }
}
