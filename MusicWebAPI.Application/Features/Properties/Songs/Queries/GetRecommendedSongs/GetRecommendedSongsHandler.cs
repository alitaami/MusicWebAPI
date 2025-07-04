﻿using MediatR;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using System.Text.Json;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;

namespace MusicWebAPI.Application.Features.Properties.Songs.Queries.GetRecommendedSongs
{
    public class GetRecommendedSongsHandler : IRequestHandler<GetRecommendedSongsQuery, List<GetSongsViewModel>>
    {
        private readonly IServiceManager _serviceManager; 

        public GetRecommendedSongsHandler(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task<List<GetSongsViewModel>> Handle(GetRecommendedSongsQuery request, CancellationToken cancellationToken)
        { 
            var songs = await _serviceManager.Recommendation.RecommendSongs(request.UserId, request.Count, cancellationToken);

            var result = new List<GetSongsViewModel>();

            if (!songs.Any())
                return new List<GetSongsViewModel>();

            foreach (var item in songs)
            {
                try
                {
                    // Use JSON serialization to handle the dynamic to static type conversion
                    var data = JsonSerializer.Serialize(item);
                    var viewModel = JsonSerializer.Deserialize<GetSongsViewModel>(data);

                    if (viewModel != null)
                    {
                        result.Add(viewModel);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to map song item. Error: {ex.Message}");
                }
            }
             
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
