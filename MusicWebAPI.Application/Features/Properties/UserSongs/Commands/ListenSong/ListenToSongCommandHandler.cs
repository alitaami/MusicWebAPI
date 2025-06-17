using MediatR;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Domain.External.Caching;
using MusicWebAPI.Domain.Interfaces.Services.Base;

namespace MusicWebAPI.Application.Features.Properties.UserSongs.Commands.ListenSong
{
    public class ListenToSongCommandHandler : IRequestHandler<ListenToSongCommand>
    {
        private readonly IServiceManager _serviceManager;
        private readonly ICacheService _cacheService;

        public ListenToSongCommandHandler(IServiceManager serviceManager, ICacheService cacheService)
        {
            _serviceManager = serviceManager;
            _cacheService = cacheService;
        }

        public async Task Handle(ListenToSongCommand request, CancellationToken cancellationToken)
        {
            await _serviceManager.User.ListenToSong(request.songId, cancellationToken);

            // Invalidate all song caches
            await _cacheService.RemoveByPrefixAsync(new List<string>
            {
                Resource.GetPopularSongs_CacheKey,
                Resource.GetSongsByTerm_CacheKey
            });
        }
    }
}
