using Moq;
using be_windows_cleaners.Models;
using be_windows_cleaners.Repository;
using be_windows_cleaners.Services;
using be_windows_cleaners.Validators;
using FluentAssertions;

namespace be_windows_cleaners_test
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IUserValidators> _mockUserValidators;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockUserValidators = new Mock<IUserValidators>();
            _userService = new UserService(_mockUserRepository.Object, _mockUserValidators.Object);
        }

        #region GetAllUsersAsync Tests

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnUsers_WhenRepositoryReturnsData()
        {
            // Arrange
            var expectedUsers = new List<User>
            {
                new User { Id = 1, Name = "User 1", Email = "user1@test.com", Password = "password1", Type = UserType.Lead },
                new User { Id = 2, Name = "User 2", Email = "user2@test.com", Password = "password2", Type = UserType.Customer }
            };

            _mockUserRepository.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedUsers);
            _mockUserRepository.Verify(x => x.GetAllUsersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            // Arrange
            var expectedUsers = new List<User>();

            _mockUserRepository.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockUserRepository.Verify(x => x.GetAllUsersAsync(), Times.Once);
        }

        #endregion

        #region GetUserByIdAsync Tests

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var expectedUser = new User 
            { 
                Id = userId, 
                Name = "Test User", 
                Email = "test@test.com", 
                Password = "password",
                Type = UserType.Lead
            };

            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedUser);
            _mockUserRepository.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 999;
            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().BeNull();
            _mockUserRepository.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        }

        #endregion

        #region GetUserByEmailAsync Tests

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var email = "test@test.com";
            var expectedUser = new User 
            { 
                Id = 1, 
                Name = "Test User", 
                Email = email, 
                Password = "password",
                Type = UserType.Lead
            };

            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByEmailAsync(email);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedUser);
            _mockUserRepository.Verify(x => x.GetUserByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "nonexistent@test.com";
            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetUserByEmailAsync(email);

            // Assert
            result.Should().BeNull();
            _mockUserRepository.Verify(x => x.GetUserByEmailAsync(email), Times.Once);
        }

        #endregion

        #region CreateUserAsync Tests

        [Fact]
        public async Task CreateUserAsync_ShouldReturnUser_WhenValidRequest()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "New User",
                Email = "newuser@test.com",
                Password = "password123",
                Type = UserType.Lead
            };

            var expectedUser = new User
            {
                Id = 1,
                Name = request.Name,
                Email = request.Email,
                Password = request.Password,
                Type = request.Type
            };

            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync((User?)null);
            _mockUserRepository.Setup(x => x.AddUserAsync(It.IsAny<User>()))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.CreateUserAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(request.Name);
            result.Email.Should().Be(request.Email);
            result.Password.Should().Be(request.Password);
            result.Type.Should().Be(request.Type);

            _mockUserValidators.Verify(x => x.ValidateCreateUserRequest(request), Times.Once);
            _mockUserRepository.Verify(x => x.GetUserByEmailAsync(request.Email), Times.Once);
            _mockUserRepository.Verify(x => x.AddUserAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowInvalidOperationException_WhenEmailAlreadyExists()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "New User",
                Email = "existing@test.com",
                Password = "password123",
                Type = UserType.Lead
            };

            var existingUser = new User
            {
                Id = 1,
                Name = "Existing User",
                Email = request.Email,
                Password = "password",
                Type = UserType.Customer
            };

            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync(existingUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _userService.CreateUserAsync(request));

            exception.Message.Should().Contain($"User with email '{request.Email}' already exists");

            _mockUserValidators.Verify(x => x.ValidateCreateUserRequest(request), Times.Once);
            _mockUserRepository.Verify(x => x.GetUserByEmailAsync(request.Email), Times.Once);
            _mockUserRepository.Verify(x => x.AddUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowException_WhenValidationFails()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "", // Invalid name
                Email = "invalid-email",
                Password = "123", // Too short
                Type = UserType.Lead
            };

            _mockUserValidators.Setup(x => x.ValidateCreateUserRequest(request))
                .Throws(new ArgumentException("Invalid user data"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.CreateUserAsync(request));

            exception.Message.Should().Be("Invalid user data");

            _mockUserValidators.Verify(x => x.ValidateCreateUserRequest(request), Times.Once);
            _mockUserRepository.Verify(x => x.GetUserByEmailAsync(It.IsAny<string>()), Times.Never);
            _mockUserRepository.Verify(x => x.AddUserAsync(It.IsAny<User>()), Times.Never);
        }

        #endregion

        #region UpdateUserAsync Tests

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnUpdatedUser_WhenValidRequest()
        {
            // Arrange
            var userId = 1;
            var request = new UpdateUserRequest
            {
                Name = "Updated User",
                Email = "updated@test.com",
                Password = "newpassword123",
                Type = UserType.Customer
            };

            var existingUser = new User
            {
                Id = userId,
                Name = "Original User",
                Email = "original@test.com",
                Password = "oldpassword",
                Type = UserType.Lead
            };

            var updatedUser = new User
            {
                Id = userId,
                Name = request.Name,
                Email = request.Email,
                Password = request.Password,
                Type = request.Type
            };

            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync((User?)null);
            _mockUserRepository.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(updatedUser);

            // Act
            var result = await _userService.UpdateUserAsync(userId, request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(request.Name);
            result.Email.Should().Be(request.Email);
            result.Password.Should().Be(request.Password);
            result.Type.Should().Be(request.Type);

            _mockUserValidators.Verify(x => x.ValidateUpdateUserRequest(request), Times.Once);
            _mockUserRepository.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
            _mockUserRepository.Verify(x => x.GetUserByEmailAsync(request.Email), Times.Once);
            _mockUserRepository.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 999;
            var request = new UpdateUserRequest
            {
                Name = "Updated User",
                Email = "updated@test.com",
                Password = "newpassword123",
                Type = UserType.Customer
            };

            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.UpdateUserAsync(userId, request);

            // Assert
            result.Should().BeNull();

            _mockUserValidators.Verify(x => x.ValidateUpdateUserRequest(request), Times.Once);
            _mockUserRepository.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
            _mockUserRepository.Verify(x => x.GetUserByEmailAsync(It.IsAny<string>()), Times.Never);
            _mockUserRepository.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrowInvalidOperationException_WhenEmailAlreadyExists()
        {
            // Arrange
            var userId = 1;
            var request = new UpdateUserRequest
            {
                Name = "Updated User",
                Email = "existing@test.com",
                Password = "newpassword123",
                Type = UserType.Customer
            };

            var existingUser = new User
            {
                Id = userId,
                Name = "Original User",
                Email = "original@test.com",
                Password = "oldpassword",
                Type = UserType.Lead
            };

            var userWithEmail = new User
            {
                Id = 2,
                Name = "Other User",
                Email = request.Email,
                Password = "password",
                Type = UserType.Customer
            };

            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync(userWithEmail);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _userService.UpdateUserAsync(userId, request));

            exception.Message.Should().Contain($"User with email '{request.Email}' already exists");

            _mockUserValidators.Verify(x => x.ValidateUpdateUserRequest(request), Times.Once);
            _mockUserRepository.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
            _mockUserRepository.Verify(x => x.GetUserByEmailAsync(request.Email), Times.Once);
            _mockUserRepository.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldNotCheckEmail_WhenEmailIsNotChanged()
        {
            // Arrange
            var userId = 1;
            var email = "same@test.com";
            var request = new UpdateUserRequest
            {
                Name = "Updated User",
                Email = email,
                Password = "newpassword123",
                Type = UserType.Customer
            };

            var existingUser = new User
            {
                Id = userId,
                Name = "Original User",
                Email = email,
                Password = "oldpassword",
                Type = UserType.Lead
            };

            var updatedUser = new User
            {
                Id = userId,
                Name = request.Name,
                Email = request.Email,
                Password = request.Password,
                Type = request.Type
            };

            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _mockUserRepository.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(updatedUser);

            // Act
            var result = await _userService.UpdateUserAsync(userId, request);

            // Assert
            result.Should().NotBeNull();

            _mockUserValidators.Verify(x => x.ValidateUpdateUserRequest(request), Times.Once);
            _mockUserRepository.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
            _mockUserRepository.Verify(x => x.GetUserByEmailAsync(It.IsAny<string>()), Times.Never);
            _mockUserRepository.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrowException_WhenValidationFails()
        {
            // Arrange
            var userId = 1;
            var request = new UpdateUserRequest
            {
                Name = "", // Invalid name
                Email = "invalid-email",
                Password = "123", // Too short
                Type = UserType.Lead
            };

            _mockUserValidators.Setup(x => x.ValidateUpdateUserRequest(request))
                .Throws(new ArgumentException("Invalid user data"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.UpdateUserAsync(userId, request));

            exception.Message.Should().Be("Invalid user data");

            _mockUserValidators.Verify(x => x.ValidateUpdateUserRequest(request), Times.Once);
            _mockUserRepository.Verify(x => x.GetUserByIdAsync(It.IsAny<int>()), Times.Never);
            _mockUserRepository.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }

        #endregion

        #region DeleteUserAsync Tests

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnTrue_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            _mockUserRepository.Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            result.Should().BeTrue();
            _mockUserRepository.Verify(x => x.DeleteUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 999;
            _mockUserRepository.Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync(false);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            result.Should().BeFalse();
            _mockUserRepository.Verify(x => x.DeleteUserAsync(userId), Times.Once);
        }

        #endregion
    }
}
