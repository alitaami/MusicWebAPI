using MusicWebAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Interfaces.Services
{
    public interface IUserService
    {
        Task<string> GoogleLogin(string idToken);
        Task<string> RegisterUser(User user, string password);
        Task<string> LoginUser(string email, string password);
        Task<string> ForgetPassword(string email);
        Task ResetPassword(string email, string newPassword);

        Task AddToFavorites(Guid songId, Guid userId, CancellationToken cancellationToken);
        Task<List<object>> GetUserFavorites(Guid userId, CancellationToken cancellationToken);
        Task DeleteFromFavorites(Guid favoriteId, CancellationToken cancellationToken);
        Task AddToPlaylist(Guid songId, Guid userId, Guid? playlistId, string playlistName, CancellationToken cancellationToken);
        Task<List<object>> GetPlaylists(Guid userId, CancellationToken cancellationToken);
        Task DeletePlayList(Guid playListId, CancellationToken cancellationToken);
        Task DeleteSongFromPlayList(Guid songId, Guid playListId, CancellationToken cancellationToken);
        Task ListenToSong(Guid songId, CancellationToken cancellationToken);
    }
}
