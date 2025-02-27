using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Domain.Entities;
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

        public UserService(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

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
    }
}
