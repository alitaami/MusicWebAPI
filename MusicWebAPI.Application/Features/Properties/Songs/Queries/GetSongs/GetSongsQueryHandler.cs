﻿using MediatR;
using MusicWebAPI.Core;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Domain.External.Caching;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using System.Text.Json;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;

namespace MusicWebAPI.Application.Features.Properties.Songs.Queries.GetSongs
{
    public class GetSongsQueryHandler : IRequestHandler<GetSongsQuery, PaginatedResult<GetSongsViewModel>>
    {
        private readonly IServiceManager _serviceManager;
        private readonly ICacheService _cacheService;

        public GetSongsQueryHandler(IServiceManager serviceManager, ICacheService cacheService)
        {
            _serviceManager = serviceManager;
            _cacheService = cacheService;
        }

        public async Task<PaginatedResult<GetSongsViewModel>> Handle(GetSongsQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"{Resource.GetSongsByTerm_CacheKey}{request.Term}_{request.PageNumber}_{request.PageSize}";

            // Try getting cached view models directly
            var cachedData = await _cacheService.GetAsync<PaginatedResult<GetSongsViewModel>>(cacheKey);

            if (cachedData != null)
            {
                return cachedData;
            }

            var songs = await _serviceManager.Home.GetSongs(request.Term, request.PageSize, request.PageNumber, cancellationToken);

            var mappedSongs = new List<GetSongsViewModel>();

            if (songs.Items is null)
                return new PaginatedResult<GetSongsViewModel>();

            foreach (var item in songs.Items)
            {
                try
                {
                    // Use JSON serialization to handle the dynamic to static type conversion
                    var data = JsonSerializer.Serialize(item);
                    var viewModel = JsonSerializer.Deserialize<GetSongsViewModel>(data);

                    if (viewModel != null)
                    {
                        mappedSongs.Add(viewModel);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to map song item. Error: {ex.Message}");
                }
            }

            var result = new PaginatedResult<GetSongsViewModel>
            {
                Items = mappedSongs,
                TotalCount = songs.TotalCount,
                PageNumber = songs.PageNumber,
                PageSize = songs.PageSize
            };

            // Cache the final mapped result
            await _cacheService.SetAsync(cacheKey, result, 60, Resource.GetSongsByTerm_CacheKey); // Adjust cache duration as needed

            return result;
        }

        // Not useful anymore
        private GetSongsViewModel MapToGetSongsViewModel(object item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var type = item.GetType();

            return new GetSongsViewModel
            {
                Id = (Guid)(type.GetProperty("Id")?.GetValue(item) ?? Guid.Empty),
                Title = type.GetProperty("Title")?.GetValue(item)?.ToString(),
                AudioUrl = type.GetProperty("AudioUrl")?.GetValue(item)?.ToString(),
                AlbumTitle = type.GetProperty("AlbumTitle")?.GetValue(item)?.ToString(),
                GenreName = type.GetProperty("GenreName")?.GetValue(item)?.ToString(),
                ArtistName = type.GetProperty("ArtistName")?.GetValue(item)?.ToString(),
                Listens = Convert.ToInt64(type.GetProperty("Listens")?.GetValue(item) ?? 0),
                Rank = Convert.ToSingle(type.GetProperty("Rank")?.GetValue(item) ?? 0f)
            };
        }
    }
}
