using MediatR;
using Microsoft.AspNetCore.Identity;
using MusicWebAPI.Application.Commands;
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
