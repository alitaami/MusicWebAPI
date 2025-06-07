using MediatR;
using Microsoft.AspNetCore.Identity;
using MusicWebAPI.Application.Commands;
using MusicWebAPI.Application.Features.Properties.Commands.UserSongs;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;

namespace MusicWebAPI.Application.Features.Properties.Commands.Handlers
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
