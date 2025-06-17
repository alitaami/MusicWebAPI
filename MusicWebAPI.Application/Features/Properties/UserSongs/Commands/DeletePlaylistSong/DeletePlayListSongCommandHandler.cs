using MediatR;
using MusicWebAPI.Domain.Interfaces.Services.Base;

namespace MusicWebAPI.Application.Features.Properties.UserSongs.Commands.DeletePlaylistSong
{
    public class DeletePlayListSongCommandHandler : IRequestHandler<DeletePlayListSongCommand>
    {
        private readonly IServiceManager _serviceManager;

        public DeletePlayListSongCommandHandler(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task Handle(DeletePlayListSongCommand request, CancellationToken cancellationToken)
        {
            await _serviceManager.User.DeleteSongFromPlayList(request.songId, request.playListId, cancellationToken);
        }
    }
}
