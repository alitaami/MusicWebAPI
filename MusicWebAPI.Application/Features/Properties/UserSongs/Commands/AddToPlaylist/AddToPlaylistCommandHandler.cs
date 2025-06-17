using MediatR;
using MusicWebAPI.Domain.Interfaces.Services.Base;

namespace MusicWebAPI.Application.Features.Properties.UserSongs.Commands.AddToPlaylist
{
    public class AddToPlaylistCommandHandler : IRequestHandler<AddToPlaylistCommand>
    {
        private readonly IServiceManager _serviceManager;

        public AddToPlaylistCommandHandler(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task Handle(AddToPlaylistCommand request, CancellationToken cancellationToken)
        {
            await _serviceManager.User.AddToPlaylist(request.SongId, request.UserId, request.PlaylistId, request.PlaylistName, cancellationToken);
        }
    }
}
