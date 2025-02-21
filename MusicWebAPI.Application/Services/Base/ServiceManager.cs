using AutoMapper;
using Mappings.CustomMapping;
using MusicWebAPI.Domain.Interfaces.Repositories;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.Services.Base
{
    public class ServiceManager : IServiceManager
    {
        private IMapper _mapper;
        private IRepositoryManager _repositoryManager;

        //private IUserService _userService;

        public ServiceManager(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddCustomMappingProfile();
            });
            IMapper mapper = mapperConfig.CreateMapper();
            _mapper = mapper;
        }

        //public IAdminService Admin
        //{
        //    get
        //    {
        //        if (this._adminService == null)
        //            this._adminService = new AdminService(_repositoryManager, _mapper);

        //        return this._adminService;
        //    }
        //}
    }
}
