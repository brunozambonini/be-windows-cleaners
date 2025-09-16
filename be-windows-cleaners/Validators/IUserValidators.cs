using be_windows_cleaners.Models;

namespace be_windows_cleaners.Validators
{
    public interface IUserValidators
    {
        void ValidateCreateUserRequest(CreateUserRequest request);
        void ValidateUpdateUserRequest(UpdateUserRequest request);
        void ValidateUserEmail(string email);
        void ValidateUserPassword(string password);
        void ValidateUserName(string name);
    }
}
