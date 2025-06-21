using MediatR;
using MusicWebAPI.Application.Features.Properties.UserSongs.Commands.AddToFavorites;
using MusicWebAPI.Domain.Interfaces.Services.Base;

namespace MusicWebAPI.Application.Features.Properties.UserSongs.Commands.AddToFavorites
{
    public class AddToFavoritesCommandHandler : IRequestHandler<AddToFavoritesCommand>
    {
        private readonly IServiceManager _serviceManager;

        public AddToFavoritesCommandHandler(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task Handle(AddToFavoritesCommand request, CancellationToken cancellationToken)
        {
            await _serviceManager.User.AddToFavorites(request.songId, request.userId, cancellationToken);
        }
    }

}
