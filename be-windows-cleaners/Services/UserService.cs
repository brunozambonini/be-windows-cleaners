using be_windows_cleaners.Models;
using be_windows_cleaners.Repository;
using be_windows_cleaners.Validators;

namespace be_windows_cleaners.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserValidators _userValidators;

        public UserService(IUserRepository userRepository, IUserValidators userValidators)
        {
            _userRepository = userRepository;
            _userValidators = userValidators;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task<User> CreateUserAsync(CreateUserRequest request)
        {
            // Validate request
            _userValidators.ValidateCreateUserRequest(request);

            // Check if email already exists
            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email '{request.Email}' already exists");
            }

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = request.Password, // In production, this should be hashed
                Type = request.Type
            };

            return await _userRepository.AddUserAsync(user);
        }

        public async Task<User?> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            // Validate request
            _userValidators.ValidateUpdateUserRequest(request);

            var existingUser = await _userRepository.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return null;
            }

            // Check if email is being changed and if new email already exists
            if (request.Email != existingUser.Email)
            {
                var userWithEmail = await _userRepository.GetUserByEmailAsync(request.Email);
                if (userWithEmail != null)
                {
                    throw new InvalidOperationException($"User with email '{request.Email}' already exists");
                }
            }

            existingUser.Name = request.Name;
            existingUser.Email = request.Email;
            existingUser.Password = request.Password; // In production, this should be hashed
            existingUser.Type = request.Type;

            return await _userRepository.UpdateUserAsync(existingUser);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepository.DeleteUserAsync(id);
        }

    }
}
