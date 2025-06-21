using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.Features.Properties.UserSongs.Commands.DeleteFromFavorites
{
    public record DeleteFromFavoritesCommand(Guid favoriteId) : IRequest;
}
