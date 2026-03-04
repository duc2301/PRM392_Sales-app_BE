using System.Text;
using System.Text.Json;
using Services.Utils;

namespace Services.Utils
{
    public static class TokenHelper
    {
        /// <summary>
        /// Decode token string và trả về user data
        /// Dùng ở Android để decode token sau khi login
        /// </summary>
        public static TokenPayload? DecodeToken(string token)
        {
            try
            {
                var tokenBytes = Convert.FromBase64String(token);
                var json = Encoding.UTF8.GetString(tokenBytes);
                var payload = JsonSerializer.Deserialize<TokenPayload>(json);
                
                // Check if token expired
                if (payload?.ExpiresAt < DateTime.UtcNow)
                    return null;
                
                return payload;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Check if token is still valid
        /// </summary>
        public static bool IsTokenValid(string token)
        {
            var payload = DecodeToken(token);
            return payload != null && payload.ExpiresAt > DateTime.UtcNow;
        }

        /// <summary>
        /// Get user ID from token
        /// </summary>
        public static int? GetUserIdFromToken(string token)
        {
            var payload = DecodeToken(token);
            return payload?.UserId;
        }

        /// <summary>
        /// Get username from token
        /// </summary>
        public static string? GetUsernameFromToken(string token)
        {
            var payload = DecodeToken(token);
            return payload?.Username;
        }

        /// <summary>
        /// Get email from token
        /// </summary>
        public static string? GetEmailFromToken(string token)
        {
            var payload = DecodeToken(token);
            return payload?.Email;
        }

        /// <summary>
        /// Get user role from token
        /// </summary>
        public static string? GetRoleFromToken(string token)
        {
            var payload = DecodeToken(token);
            return payload?.Role;
        }

        /// <summary>
        /// Get time remaining before token expires (in minutes)
        /// </summary>
        public static int? GetTokenExpiryMinutes(string token)
        {
            var payload = DecodeToken(token);
            if (payload == null)
                return null;

            var timeRemaining = payload.ExpiresAt - DateTime.UtcNow;
            return (int)timeRemaining.TotalMinutes;
        }
    }
}
