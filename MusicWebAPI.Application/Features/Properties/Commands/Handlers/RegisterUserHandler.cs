using MediatR;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using MusicWebAPI.Application.Commands;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System;
using static MusicWebAPI.Application.ViewModels.UserViewModel;
using AutoMapper;
using MusicWebAPI.Application.Validators;

namespace MusicWebAPI.Application.Commands.Handlers
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserViewModel>
    {
        private readonly IServiceManager _serviceManager;
        private readonly IMapper _mapper;
        public RegisterUserHandler(IServiceManager serviceManager, IMapper mapper)
        {
            _serviceManager = serviceManager;
            _mapper = mapper;
        }

        public async Task<RegisterUserViewModel> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
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
