using MediatR;
using MusicWebAPI.Application.Commands;
using MusicWebAPI.Core;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;
using static MusicWebAPI.Application.ViewModels.UserViewModel;

namespace MusicWebAPI.Application.Features.Properties.Queries.Handlers
{
    public class GetSongsQueryHandler : IRequestHandler<GetSongsQuery, PaginatedResult<GetSongsViewModel>>
    {
        private readonly IServiceManager _serviceManager;

        public GetSongsQueryHandler(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task<PaginatedResult<GetSongsViewModel>> Handle(GetSongsQuery request, CancellationToken cancellationToken)
        {
            var songs = await _serviceManager.Home.GetSongs(request.Term, request.PageSize, request.PageNumber, cancellationToken);

            if (songs.Items == null || !songs.Items.Any())
            {
                return new PaginatedResult<GetSongsViewModel>
                {
                    Items = new List<GetSongsViewModel>(),
                    TotalCount = 0,
                    PageNumber = songs.PageNumber,
                    PageSize = songs.PageSize
                };
            }

            var mappedSongs = new List<GetSongsViewModel>();

            foreach (var item in songs.Items)
            {
                try
                {
                    var viewModel = MapToGetSongsViewModel(item);
                    mappedSongs.Add(viewModel);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to map song item. Error: {ex.Message}");
                }
            }

            return new PaginatedResult<GetSongsViewModel>
            {
                Items = mappedSongs,
                TotalCount = songs.TotalCount,
                PageNumber = songs.PageNumber,
                PageSize = songs.PageSize
            };
        }

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
                Rank = Convert.ToSingle(type.GetProperty("Rank")?.GetValue(item) ?? 0f)
            };
        }
    }
}
