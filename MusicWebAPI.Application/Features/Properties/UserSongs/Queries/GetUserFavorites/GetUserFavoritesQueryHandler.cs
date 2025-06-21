using MediatR;
using MusicWebAPI.Application.Features.Properties.UserSongs.Queries.GetPlaylists;
using MusicWebAPI.Core;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static MusicWebAPI.Application.DTOs.UserSongsDTO;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;
using static MusicWebAPI.Application.ViewModels.SongsViewModel;

namespace MusicWebAPI.Application.Features.Properties.UserSongs.Queries.GetUserFavorites
{
    public class GetUserFavoritesQueryHandler : IRequestHandler<GetUserFavoritesQuery, List<UserFavoriteViewModel>>
    {
        private readonly IServiceManager _serviceManager;

        public GetUserFavoritesQueryHandler(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task<List<UserFavoriteViewModel>> Handle(GetUserFavoritesQuery request, CancellationToken cancellationToken)
        {
            var userFavorites = await _serviceManager.User.GetUserFavorites(request.userId, cancellationToken);
            var mappedFavorites = new List<UserFavoriteViewModel>();

            if (mappedFavorites is null)
                return new List<UserFavoriteViewModel>();

            foreach (var item in userFavorites)
            {
                try
                {
                    // Use JSON serialization to handle the dynamic to static type conversion
                    var data = JsonSerializer.Serialize(item);
                    var viewModel = JsonSerializer.Deserialize<UserFavoriteViewModel>(data);

                    if (viewModel != null)
                    {
                        mappedFavorites.Add(viewModel);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to map user favorites item. Error: {ex.Message}");
                }
            }

            return mappedFavorites;
        }
    }
}
