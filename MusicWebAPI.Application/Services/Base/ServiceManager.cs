using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MusicWebAPI.Application.Services;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Repositories;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using MusicWebAPI.Domain.Interfaces.Services;
using Mappings.CustomMapping;

public class ServiceManager : IServiceManager
{
    private readonly IMapper _mapper;
    private readonly IRepositoryManager _repositoryManager;
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;

    private readonly Lazy<IUserService> _userService;

    public ServiceManager(IRepositoryManager repositoryManager, UserManager<User> userManager, IConfiguration configuration)
    {
        _repositoryManager = repositoryManager;
        _userManager = userManager;
        _configuration = configuration;

        var mapperConfig = new MapperConfiguration(config =>
        {
            config.AddCustomMappingProfile();
        });
        _mapper = mapperConfig.CreateMapper();

        // Lazy initialization for thread safety
        _userService = new Lazy<IUserService>(() => new UserService(_userManager, _mapper, _configuration));
    }

    public IUserService User => _userService.Value;
}
