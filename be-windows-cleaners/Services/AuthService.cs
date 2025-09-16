using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using be_windows_cleaners.Models;

namespace be_windows_cleaners.Services
{
    public class AuthService : IAuthService
    {
        private readonly string _secretKey;
        private readonly int _tokenExpirationHours;

        public AuthService(IConfiguration configuration)
        {
            _secretKey = configuration["JwtSettings:SecretKey"] ?? "your-super-secret-key-that-should-be-at-least-32-characters-long";
            _tokenExpirationHours = int.Parse(configuration["JwtSettings:ExpirationHours"] ?? "24");
        }

        public string GenerateToken(int userId, string email, UserType userType)
        {
            var tokenData = new
            {
                UserId = userId,
                Email = email,
                UserType = userType.ToString(),
                IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(_tokenExpirationHours).ToUnixTimeSeconds()
            };

            var tokenJson = JsonSerializer.Serialize(tokenData);
            var tokenBytes = Encoding.UTF8.GetBytes(tokenJson);
            
            // Simple encoding using HMAC-SHA256 (in production, use proper JWT)
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
            var signature = hmac.ComputeHash(tokenBytes);
            
            var token = Convert.ToBase64String(tokenBytes) + "." + Convert.ToBase64String(signature);
            return token;
        }

        public bool ValidateToken(string token, out int userId)
        {
            userId = 0;
            
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 2)
                    return false;

                var tokenBytes = Convert.FromBase64String(parts[0]);
                var signature = Convert.FromBase64String(parts[1]);

                // Verify signature
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
                var expectedSignature = hmac.ComputeHash(tokenBytes);
                
                if (!signature.SequenceEqual(expectedSignature))
                    return false;

                // Parse token data
                var tokenJson = Encoding.UTF8.GetString(tokenBytes);
                var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenJson);

                // Check expiration
                if (tokenData.TryGetProperty("ExpiresAt", out var expiresAtElement))
                {
                    var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresAtElement.GetInt64());
                    if (expiresAt < DateTimeOffset.UtcNow)
                        return false;
                }

                // Extract user ID
                if (tokenData.TryGetProperty("UserId", out var userIdElement))
                {
                    userId = userIdElement.GetInt32();
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
