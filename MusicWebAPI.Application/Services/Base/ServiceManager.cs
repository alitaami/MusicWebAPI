using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MusicWebAPI.Application.Services;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using MusicWebAPI.Domain.Interfaces.Services;
using Mappings.CustomMapping;
using Minio;
using MusicWebAPI.Infrastructure.FileService;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Domain.External.Caching;

public class ServiceManager : IServiceManager
{
    private readonly IMapper _mapper;
    private readonly IRepositoryManager _repositoryManager;
    private readonly ICacheService _cacheService;
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;

    private readonly Lazy<IUserService> _userService;
    private readonly Lazy<IHomeService> _homeService;
    private readonly Lazy<IRecommendationService> _recommendService;
    public ServiceManager(IRepositoryManager repositoryManager, ICacheService cacheService, UserManager<User> userManager, IConfiguration configuration)
    {
        _cacheService = cacheService;
        _repositoryManager = repositoryManager;
        _userManager = userManager;
        _configuration = configuration;

        var mapperConfig = new MapperConfiguration(config =>
        {
            config.AddCustomMappingProfile();
        });
        _mapper = mapperConfig.CreateMapper();

        // Lazy initialization for thread safety
        _userService = new Lazy<IUserService>(() => new UserService(_userManager, repositoryManager, _mapper, _configuration));
        _homeService = new Lazy<IHomeService>(() => new HomeService(_repositoryManager, _mapper));
        _recommendService = new Lazy<IRecommendationService>(() => new RecommendationService(_repositoryManager, cacheService));
    }

    public IUserService User => _userService.Value;
    public IHomeService Home => _homeService.Value;
    public IRecommendationService Recommendation => _recommendService.Value;
}