using MediatR;
using MusicWebAPI.Domain.Interfaces.Services.Base;

namespace MusicWebAPI.Application.Features.Properties.UserSongs.Commands.DeletePlaylist
{
    public class DeletePlayListCommandHandler : IRequestHandler<DeletePlayListCommand>
    {
        private readonly IServiceManager _serviceManager;

        public DeletePlayListCommandHandler(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task Handle(DeletePlayListCommand request, CancellationToken cancellationToken)
        {
            await _serviceManager.User.DeletePlayList(request.playListId, cancellationToken);
        }
    }

}
