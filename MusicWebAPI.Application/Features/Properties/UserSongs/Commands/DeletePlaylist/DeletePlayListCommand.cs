using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Application.ViewModels.SongsViewModel;

namespace MusicWebAPI.Application.Features.Properties.UserSongs.Commands.DeletePlaylist
{
    public record DeletePlayListCommand(Guid playListId) : IRequest;

}
