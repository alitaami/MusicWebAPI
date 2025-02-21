using AutoMapper;
using Mappings.CustomMapping;
using Microsoft.Extensions.Configuration;
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
        private IMapper _mapper;
        private IRepositoryManager _repositoryManager;
        private readonly IConfiguration _configuration;

        private IJwtService _jwtService;

        public ServiceManager(IRepositoryManager repositoryManager, IConfiguration configuration)
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

        public IJwtService Jwt
        {
            get
            {
                if (this._jwtService == null)
                    this._jwtService = new JwtService(_configuration);

                return this._jwtService;
            }
        }
    }
}
