using AutoMapper;
using Common.Utilities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Core.Utilities;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.External.Caching;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Domain.Interfaces.Services;
using System.Net;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;

namespace MusicWebAPI.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IRepositoryManager _repositoryManager;
        private readonly string _googleClientId;
        private readonly string _resetPassUrl;

        public UserService(UserManager<User> userManager, IRepositoryManager repositoryManager, IMapper mapper, IConfiguration configuration)
        {
            _repositoryManager = repositoryManager;
            _configuration = configuration;
            _userManager = userManager;
            _mapper = mapper;
            _googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
            _resetPassUrl = Environment.GetEnvironmentVariable("RESETPASS_API_URL");
        }

        #region Authorization & Authentication

        public async Task<string> GoogleLogin(string idToken)
        {
            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { _googleClientId }
                });
            }
            catch
            {
                throw new UnauthorizedException(Resource.InvalidGoogleToken);
            }

            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                user = new User
                {
                    Email = payload.Email,
                    UserName = payload.Email,
                    FullName = payload.Name,
                    EmailConfirmed = true,
                    Avatar = payload.Picture,
                    IsArtist = false,
                };

                var result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                    throw new BadRequestException(Resource.UserRegistrationError);

                await AssignRole(user, false);
            }

            return JwtHelper.GenerateToken(user, _configuration);
        }

        public async Task<string> RegisterUser(User user, string password)
        {
            if (await IsUserExists(user.Email, user.UserName))
                throw new LogicException(Resource.DuplicateUserError);

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                throw new BadRequestException(Resource.UserRegistrationError);

            await AssignRole(user, user.IsArtist);

            var token = JwtHelper.GenerateToken(user, _configuration);

            return token;
        }

        public async Task<string> LoginUser(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new UnauthorizedException(Resource.InvalidCredentials);

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
                throw new UnauthorizedException(Resource.InvalidCredentials);

            return JwtHelper.GenerateToken(user, _configuration);
        }

        public async Task<string> ForgetPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new NotFoundException(Resource.UserNotFound);

            var otp = Tools.GenerateOtp();

            string resetPassUrl = $"{_resetPassUrl}?email={email}";

            string body = string.Format(Resource.ForgetPassEmailBody, otp, resetPassUrl);

            await SendMail.SendAsync(email, "Password Reset", body);

            return otp;
        }

        public async Task ResetPassword(string email, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new NotFoundException(Resource.UserNotFound);

            // Remove old password & set new one 
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
                throw new LogicException(Resource.ResetPasswordError);
        }

        #endregion

        public async Task AddToFavorites(Guid songId, Guid userId, CancellationToken cancellationToken)
        {
            var favoriteExists = await _repositoryManager
                .UserFavorite
                .Get(cancellationToken)
                .AsNoTracking()
                .AnyAsync(f => f.SongId == songId && f.UserId == userId.ToString(), cancellationToken);

            if (favoriteExists)
                return;

            await _repositoryManager.UserFavorite.AddAsync(new UserFavorite
            {
                SongId = songId,
                UserId = userId.ToString(),
            }, cancellationToken, saveNow: true);
        }

        public async Task DeleteFromFavorites(Guid favoriteId, CancellationToken cancellationToken)
        {
            var favorite = await _repositoryManager.UserFavorite.GetByIdAsync(cancellationToken, favoriteId);

            if (favorite != null)
            {
                await _repositoryManager.UserFavorite.DeleteAsync(favorite, cancellationToken, saveNow: true);
            }
        }

        public async Task<List<object>> GetUserFavorites(Guid userId, CancellationToken cancellationToken)
        {
            return await _repositoryManager
                         .UserFavorite
                         .Get(cancellationToken)
                         .AsNoTracking()
                         .Where(f => f.UserId == userId.ToString())
                         .Select(f => (object)new
                         {
                             Id = f.Id,
                             UserId = f.UserId,
                             SongId = f.SongId,
                             SongName = f.Song.Title
                         })
                         .ToListAsync(cancellationToken);
        }

        public async Task AddToPlaylist(Guid songId, Guid userId, Guid? playlistId, string playlistName, CancellationToken cancellationToken)
        {
            var playList = new Playlist();
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var song = await _repositoryManager.Song.GetByIdAsync(cancellationToken, songId);

            if (user == null)
                throw new NotFoundException(Resource.UserNotFound);

            if (song == null)
                throw new NotFoundException(Resource.SongNotFound);

            if (playlistId.HasValue)
            {
                playList = await _repositoryManager.PlayList.GetPlayList((Guid)playlistId, cancellationToken);
            }
            else
            {
                playList = new Playlist
                {
                    Name = playlistName,
                    UserId = userId.ToString(),
                    CreatedByUserId = userId,
                };

                playList = await _repositoryManager.PlayList.CreatePlaylist(playList, cancellationToken);
            }

            await _repositoryManager.PlayListSongs.AddSongsToPlayList(songId, playList.Id, cancellationToken);
        }

        public async Task<List<object>> GetPlaylists(Guid userId, CancellationToken cancellationToken)
        {
            return await _repositoryManager.PlayList.GetUserPlaylist(userId, cancellationToken);
        }

        public async Task DeletePlayList(Guid playListId, CancellationToken cancellationToken)
        {
            await _repositoryManager.PlayList.DeletePlayList(playListId, cancellationToken);
        }

        public async Task DeleteSongFromPlayList(Guid songId, Guid playListId, CancellationToken cancellationToken)
        {
            await _repositoryManager.PlayListSongs.DeleteSongFromPlayList(songId, playListId, cancellationToken);
        }

        public async Task ListenToSong(Guid songId, CancellationToken cancellationToken)
        {
            var song = await _repositoryManager.Song.GetByIdAsync(cancellationToken, songId);

            if (song == null)
                throw new NotFoundException(Resource.SongNotFound);

            song.Listens++;

            await _repositoryManager.Song.UpdateAsync(song, cancellationToken, saveNow: true);
        }

        #region Common
        private async Task<bool> IsUserExists(string email, string username)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null) return true;

            existingUser = await _userManager.FindByNameAsync(username);
            if (existingUser != null) return true;

            return false;
        }

        private async Task AssignRole(User user, bool? isArtist = false)
        {
            if (isArtist == null)
            {
                await _userManager.AddToRoleAsync(user, Resource.SuperUserRole);
            }
            else
            {
                if (isArtist.Value)
                {
                    await _userManager.AddToRoleAsync(user, Resource.ArtistRole);
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, Resource.UserRole);
                }
            }
        }
        #endregion
    }
}
