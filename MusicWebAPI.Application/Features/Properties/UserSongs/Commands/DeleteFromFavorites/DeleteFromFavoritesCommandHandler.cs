using MediatR;
using MusicWebAPI.Application.Features.Properties.UserSongs.Commands.AddToFavorites;
using MusicWebAPI.Domain.Interfaces.Services.Base;

namespace MusicWebAPI.Application.Features.Properties.UserSongs.Commands.DeleteFromFavorites
{
    public class DeleteFromFavoritesCommandHandler : IRequestHandler<DeleteFromFavoritesCommand>
    {
        private readonly IServiceManager _serviceManager;

        public DeleteFromFavoritesCommandHandler(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task Handle(DeleteFromFavoritesCommand request, CancellationToken cancellationToken)
        {
            await _serviceManager.User.DeleteFromFavorites(request.favoriteId, cancellationToken);
        }
    }

}
