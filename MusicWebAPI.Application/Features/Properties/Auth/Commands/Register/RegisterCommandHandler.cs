using MediatR;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System;
using static MusicWebAPI.Application.ViewModels.UserViewModel;
using AutoMapper;
using MusicWebAPI.Application.Validators;

namespace MusicWebAPI.Application.Features.Properties.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterUserViewModel>
    {
        private readonly IServiceManager _serviceManager;
        private readonly IMapper _mapper;
        public RegisterCommandHandler(IServiceManager serviceManager, IMapper mapper)
        {
            _serviceManager = serviceManager;
            _mapper = mapper;
        }

        public async Task<RegisterUserViewModel> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Call the UserService to register the user
            var token = await _serviceManager.User.RegisterUser(new User
            {
                UserName = request.UserName,
                Email = request.Email,
                FullName = request.FullName,
                IsArtist = request.IsArtist
            }, request.Password);

            return new RegisterUserViewModel
            {
                Token = token
            };
        }
    }
}
