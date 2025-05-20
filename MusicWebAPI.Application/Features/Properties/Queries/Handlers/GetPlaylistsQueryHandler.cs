using MediatR;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Application.DTOs.UserSongsDTO;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;
using static MusicWebAPI.Application.ViewModels.SongsViewModel;

namespace MusicWebAPI.Application.Features.Properties.Queries.Handlers
{
    public class GetPlaylistsQueryHandler : IRequestHandler<GetPlaylistsQuery, List<PlaylistViewModel>>
    {
        private readonly IServiceManager _serviceManager;

        public GetPlaylistsQueryHandler(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task<List<PlaylistViewModel>> Handle(GetPlaylistsQuery request, CancellationToken cancellationToken)
        {
            var playlists = await _serviceManager.User.GetPlaylists(request.UserId, cancellationToken);
            var mappedPlaylist = new List<PlaylistViewModel>();

            foreach (var item in playlists)
            {
                try
                {
                    var viewModel = MapToGetPlaylistViewModel(item);

                    mappedPlaylist.Add(viewModel);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to map song item. Error: {ex.Message}");
                }
            }

            return mappedPlaylist ?? new List<PlaylistViewModel>() { };
        }
        private PlaylistViewModel MapToGetPlaylistViewModel(object item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var type = item.GetType();

            return new PlaylistViewModel
            {
                PlayListId = (Guid)(type.GetProperty("PlayListId")?.GetValue(item) ?? Guid.Empty),
                Name = type.GetProperty("Name")?.GetValue(item)?.ToString(),
                CreatedByUserId = (Guid)(type.GetProperty("CreatedByUserId")?.GetValue(item) ?? Guid.Empty),
                UserId = (Guid)(type.GetProperty("UserId")?.GetValue(item) ?? Guid.Empty),
                Songs = (type.GetProperty("Songs")?.GetValue(item) as List<Guid>) ?? new List<Guid>()
            };
        }
    }
}
