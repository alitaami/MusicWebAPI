﻿using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Application.DTOs.UserSongsDTO;
using static MusicWebAPI.Application.ViewModels.SongsViewModel;

namespace MusicWebAPI.Application.Features.Properties.UserSongs.Queries.GetPlaylists
{
    public record GetUserFavoritesQuery(Guid userId) : IRequest<List<UserFavoriteViewModel>>;
}
