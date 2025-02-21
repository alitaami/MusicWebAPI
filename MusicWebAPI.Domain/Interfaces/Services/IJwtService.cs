using MusicWebAPI.Domain.Entities;

namespace MusicWebAPI.Domain.Interfaces.Services
{
    public interface IJwtService
    {
        /// <summary>
        /// Generates a JWT token for a given user.
        /// </summary>
        /// <param name="user">The application user.</param>
        /// <returns>A signed JWT token as string.</returns>
        string GenerateToken(User user);
    }
}
