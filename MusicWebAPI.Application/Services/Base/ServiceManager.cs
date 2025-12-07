using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MusicWebAPI.Application.Services;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using MusicWebAPI.Domain.Interfaces.Services;
using Mappings.CustomMapping;
using Minio;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Domain.External.Caching;
using MusicWebAPI.Domain.Interfaces.Services.External;

public class ServiceManager : IServiceManager
{
    private readonly IMapper _mapper;
    private readonly IRepositoryManager _repositoryManager;
    private readonly ICacheService _cacheService;
    private readonly IRedisLockFactory _redisLock;
    private readonly IOutboxService _outboxService;
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    private readonly HttpClient _httpClient;

    private readonly Lazy<IUserService> _userService;
    private readonly Lazy<ISongService> _homeService;
    private readonly Lazy<IRecommendationService> _recommendService;
    private readonly Lazy<ISubscriptionService> _subscriptionService;
    public ServiceManager(IRepositoryManager repositoryManager, ICacheService cacheService, IRedisLockFactory redisLock, IOutboxService outboxService, UserManager<User> userManager, IConfiguration configuration, HttpClient httpClient)
    {
        _cacheService = cacheService;
        _redisLock = redisLock;
        _outboxService = outboxService;
        _repositoryManager = repositoryManager;
        _userManager = userManager;
        _configuration = configuration;
        _httpClient = httpClient;

        var mapperConfig = new MapperConfiguration(config =>
        {
            config.AddCustomMappingProfile();
        });
        _mapper = mapperConfig.CreateMapper();

        // Lazy initialization for thread safety
        _userService = new Lazy<IUserService>(() => new UserService(_userManager, _repositoryManager, _outboxService, _mapper, _configuration));
        _homeService = new Lazy<ISongService>(() => new SongService(_repositoryManager, _mapper));
        _recommendService = new Lazy<IRecommendationService>(() => new RecommendationService(_repositoryManager, _cacheService, _configuration, _httpClient));
        _subscriptionService = new Lazy<ISubscriptionService>(() => new SubscriptionService(_repositoryManager, _outboxService, _redisLock));
    }

    public IUserService User => _userService.Value;
    public ISongService Home => _homeService.Value;
    public IRecommendationService Recommendation => _recommendService.Value;
    public ISubscriptionService Subscription => _subscriptionService.Value;
}