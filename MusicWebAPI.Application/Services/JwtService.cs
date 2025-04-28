using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MusicWebAPI.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public static class JwtHelper
{
    public static string GenerateToken(User user, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("JwtSettings"); // Match with JSON
        var secretKey = jwtSection["SecretKey"]; // Fix key name
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new ArgumentException("JWT Key cannot be null or empty.");
        }

        // Check if the secret key is less than 128 bits, and pad if necessary
        if (secretKey.Length < 16)
        {
            throw new ArgumentException("JWT Key must be at least 128 bits (16 characters long).");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var signinCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.IsArtist ? "Artist" : "User")
    };

        var tokenOptions = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: signinCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }
}
