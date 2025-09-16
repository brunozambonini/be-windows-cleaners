using be_windows_cleaners.Models;

namespace be_windows_cleaners.Services
{
    public interface IAuthService
    {
        string GenerateToken(int userId, string email, UserType userType);
        bool ValidateToken(string token, out int userId);
    }
}
