using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using be_windows_cleaners.Models;

namespace be_windows_cleaners.Validators
{
    public class UserValidators : IUserValidators
    {
        public void ValidateCreateUserRequest(CreateUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "User request cannot be null");

            ValidateUserName(request.Name);
            ValidateUserEmail(request.Email);
            ValidateUserPassword(request.Password);
        }

        public void ValidateUpdateUserRequest(UpdateUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "User request cannot be null");

            ValidateUserName(request.Name);
            ValidateUserEmail(request.Email);
            ValidateUserPassword(request.Password);
        }

        public void ValidateUserEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required", nameof(email));

            if (email.Length > 255)
                throw new ArgumentException("Email cannot exceed 255 characters", nameof(email));

            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(email))
                throw new ArgumentException("Invalid email format", nameof(email));
        }

        public void ValidateUserPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required", nameof(password));

            if (password.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters long", nameof(password));

            if (password.Length > 255)
                throw new ArgumentException("Password cannot exceed 255 characters", nameof(password));
        }

        public void ValidateUserName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required", nameof(name));

            if (name.Length > 100)
                throw new ArgumentException("Name cannot exceed 100 characters", nameof(name));

            if (name.Length < 2)
                throw new ArgumentException("Name must be at least 2 characters long", nameof(name));
        }
    }
}
