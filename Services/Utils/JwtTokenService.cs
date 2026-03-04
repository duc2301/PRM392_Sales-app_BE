using System.Text;
using System.Text.Json;

namespace Services.Utils
{
    public interface ITokenService
    {
        string GenerateToken(int userId, string username, string email, string role, 
            string? phoneNumber = null, string? address = null);
    }

    public class JwtTokenService : ITokenService
    {
        private const int TokenExpiryHours = 24;

        public string GenerateToken(int userId, string username, string email, string role, 
            string? phoneNumber = null, string? address = null)
        {
            // Create token payload with user information
            var tokenPayload = new TokenPayload
            {
                UserId = userId,
                Username = username,
                Email = email,
                Role = role,
                PhoneNumber = phoneNumber,
                Address = address,
                ExpiresAt = DateTime.UtcNow.AddHours(TokenExpiryHours)
            };

            // Serialize to JSON
            var json = JsonSerializer.Serialize(tokenPayload);
            var tokenBytes = Encoding.UTF8.GetBytes(json);
            var token = Convert.ToBase64String(tokenBytes);

            return token;
        }
    }
}


