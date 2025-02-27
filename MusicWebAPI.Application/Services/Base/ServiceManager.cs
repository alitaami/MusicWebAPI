using AutoMapper;
using Mappings.CustomMapping;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Repositories;
using MusicWebAPI.Domain.Interfaces.Services;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.Services.Base
{
    public class ServiceManager : IServiceManager
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryManager _repositoryManager;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;

        private IJwtService _jwtService;
        private IUserService _userService;

        public ServiceManager(IRepositoryManager repositoryManager, UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            {
                _repositoryManager = repositoryManager;
                _configuration = configuration;

                var mapperConfig = new MapperConfiguration(config =>
                {
                    config.AddCustomMappingProfile();
                });
                IMapper mapper = mapperConfig.CreateMapper();
                _mapper = mapper;
            }
        }

        public IJwtService Jwt
        {
            get
            {
                if (this._jwtService == null)
                    this._jwtService = new JwtService(_configuration);

                return this._jwtService;
            }
        }
        public IUserService User
        {
            get
            {
                if (this._userService == null)
                    this._userService = new UserService(_userManager, _mapper);

                return this._userService;
            }
        }
    }
}
