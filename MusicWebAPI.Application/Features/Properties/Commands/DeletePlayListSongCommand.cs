using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Application.ViewModels.SongsViewModel;

namespace MusicWebAPI.Application.Features.Properties.Commands
{
    public record DeletePlayListSongCommand(Guid songId, Guid playListId) : IRequest;

}
