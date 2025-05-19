using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Domain.Interfaces.Services;
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

        public UserService(UserManager<User> userManager, IRepositoryManager repositoryManager, IMapper mapper, IConfiguration configuration)
        {
            _repositoryManager = repositoryManager;
            _configuration = configuration;
            _userManager = userManager;
            _mapper = mapper;
        }

        #region Authorization & Authentication
        public async Task<User> RegisterUser(User user, string password)
        {
            if (await IsUserExists(user.Email, user.UserName))
                throw new LogicException(Resource.DuplicateUserError);

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                throw new BadRequestException(Resource.UserRegistrationError);

            await AssignRole(user, user.IsArtist);

            return user;
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
        #endregion

        public async Task AddToPlaylist(Guid songId, Guid userId, Guid? playlistId, string playlistName, CancellationToken cancellationToken)
        {
            var playList = new Playlist();
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                throw new NotFoundException(Resource.UserNotFound);

            if (playlistId.HasValue)
            {
                playList = await _repositoryManager.PlayList.GetPlayList((Guid)playlistId, cancellationToken);
            }
            else
            {
                playList = new Playlist
                {
                    Name = playlistName,
                    UserId = userId,
                    CreatedByUserId = userId,
                };

                playList = await _repositoryManager.PlayList.CreatePlaylist(playList, cancellationToken);
            }

            await _repositoryManager.PlayListSongs.AddSongsToPlayList(songId, playList.Id, cancellationToken);
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

        private async Task AssignRole(User user, bool isArtist)
        {
            if (isArtist)
            {
                await _userManager.AddToRoleAsync(user, Resource.ArtistRole);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, Resource.UserRole);
            }
        }
        #endregion
    }
}
