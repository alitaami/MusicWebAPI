using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Application.DTOs.UserSongsDTO;
using static MusicWebAPI.Application.ViewModels.SongsViewModel;

namespace MusicWebAPI.Application.Features.Properties.Queries.UserSongs
{
    public record GetPlaylistsQuery(Guid UserId) : IRequest<List<PlaylistViewModel>>;
}
