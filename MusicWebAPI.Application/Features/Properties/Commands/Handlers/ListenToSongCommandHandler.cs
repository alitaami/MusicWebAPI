using MediatR;
using Microsoft.AspNetCore.Identity;
using MusicWebAPI.Application.Commands;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.External.Caching;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using MusicWebAPI.Infrastructure.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;

namespace MusicWebAPI.Application.Features.Properties.Commands.Handlers
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
            await _serviceManager.User.ListenToSong(request.songId, request.userId, cancellationToken);

            // Invalidate all song caches
            await _cacheService.RemoveByPrefixAsync(new List<string>
            {
                Resource.GetPopularSongs_CacheKey,
                Resource.GetSongsByTerm_CacheKey
            });
        }
    }
}
