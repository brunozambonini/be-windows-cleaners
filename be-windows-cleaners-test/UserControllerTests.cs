using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using be_windows_cleaners.Controllers;
using be_windows_cleaners.Models;
using be_windows_cleaners.Services;
using FluentAssertions;

namespace be_windows_cleaners_test
{
    public class UserControllerTests
    {
        private readonly Mock<ILogger<UserController>> _mockLogger;
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockLogger = new Mock<ILogger<UserController>>();
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockLogger.Object, _mockUserService.Object);
        }

        #region GetUsers Tests

        [Fact]
        public async Task GetUsers_ShouldReturnAllUsers_WhenUsersExist()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Name = "User 1", Email = "user1@test.com", Password = "password1", Type = UserType.Lead },
                new User { Id = 2, Name = "User 2", Email = "user2@test.com", Password = "password2", Type = UserType.Customer }
            };

            _mockUserService.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var userResponses = okResult!.Value as IEnumerable<UserResponse>;
            userResponses.Should().NotBeNull();
            userResponses.Should().HaveCount(2);
            
            var userResponseList = userResponses!.ToList();
            userResponseList[0].Id.Should().Be(1);
            userResponseList[0].Name.Should().Be("User 1");
            userResponseList[0].Email.Should().Be("user1@test.com");
            userResponseList[0].Type.Should().Be(UserType.Lead);
            
            userResponseList[1].Id.Should().Be(2);
            userResponseList[1].Name.Should().Be("User 2");
            userResponseList[1].Email.Should().Be("user2@test.com");
            userResponseList[1].Type.Should().Be(UserType.Customer);
            
            _mockUserService.Verify(x => x.GetAllUsersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetUsers_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            // Arrange
            var users = new List<User>();
            _mockUserService.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var userResponses = okResult!.Value as IEnumerable<UserResponse>;
            userResponses.Should().NotBeNull();
            userResponses.Should().BeEmpty();
            _mockUserService.Verify(x => x.GetAllUsersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetUsers_ShouldReturnInternalServerError_WhenServiceThrowsException()
        {
            // Arrange
            var exception = new Exception("Database error");
            _mockUserService.Setup(x => x.GetAllUsersAsync())
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Error retrieving users");
        }

        #endregion

        #region GetUser Tests

        [Fact]
        public async Task GetUser_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var user = new User 
            { 
                Id = userId, 
                Name = "Test User", 
                Email = "test@test.com", 
                Password = "password",
                Type = UserType.Lead,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var userResponse = okResult!.Value as UserResponse;
            userResponse.Should().NotBeNull();
            userResponse!.Id.Should().Be(userId);
            userResponse.Name.Should().Be("Test User");
            userResponse.Email.Should().Be("test@test.com");
            userResponse.Type.Should().Be(UserType.Lead);
            _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 999;
            _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be($"User with ID {userId} not found");
            _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUser_ShouldReturnInternalServerError_WhenServiceThrowsException()
        {
            // Arrange
            var userId = 1;
            var exception = new Exception("Database error");
            _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Error retrieving user");
        }

        #endregion

        #region CreateUser Tests

        [Fact]
        public async Task CreateUser_ShouldReturnCreatedResult_WhenValidRequest()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "New User",
                Email = "newuser@test.com",
                Password = "password123",
                Type = UserType.Lead
            };

            var createdUser = new User
            {
                Id = 1,
                Name = request.Name,
                Email = request.Email,
                Password = request.Password,
                Type = request.Type,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockUserService.Setup(x => x.CreateUserAsync(request))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _controller.CreateUser(request);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be(nameof(UserController.GetUser));
            createdResult.RouteValues!["id"].Should().Be(createdUser.Id);
            
            var userResponse = createdResult.Value as UserResponse;
            userResponse.Should().NotBeNull();
            userResponse!.Id.Should().Be(createdUser.Id);
            userResponse.Name.Should().Be(createdUser.Name);
            userResponse.Email.Should().Be(createdUser.Email);
            userResponse.Type.Should().Be(createdUser.Type);
            
            _mockUserService.Verify(x => x.CreateUserAsync(request), Times.Once);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "", // Invalid
                Email = "invalid-email",
                Password = "123", // Too short
                Type = UserType.Lead
            };

            _controller.ModelState.AddModelError("Name", "Name is required");
            _controller.ModelState.AddModelError("Email", "Invalid email format");
            _controller.ModelState.AddModelError("Password", "Password too short");

            // Act
            var result = await _controller.CreateUser(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            _mockUserService.Verify(x => x.CreateUserAsync(It.IsAny<CreateUserRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenArgumentExceptionThrown()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "New User",
                Email = "newuser@test.com",
                Password = "password123",
                Type = UserType.Lead
            };

            var exception = new ArgumentException("Invalid user data");
            _mockUserService.Setup(x => x.CreateUserAsync(request))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.CreateUser(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Invalid user data");
        }

        [Fact]
        public async Task CreateUser_ShouldReturnConflict_WhenInvalidOperationExceptionThrown()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "New User",
                Email = "existing@test.com",
                Password = "password123",
                Type = UserType.Lead
            };

            var exception = new InvalidOperationException("User with email 'existing@test.com' already exists");
            _mockUserService.Setup(x => x.CreateUserAsync(request))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.CreateUser(request);

            // Assert
            result.Result.Should().BeOfType<ConflictObjectResult>();
            var conflictResult = result.Result as ConflictObjectResult;
            conflictResult!.Value.Should().Be("User with email 'existing@test.com' already exists");
        }

        [Fact]
        public async Task CreateUser_ShouldReturnInternalServerError_WhenUnexpectedExceptionThrown()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "New User",
                Email = "newuser@test.com",
                Password = "password123",
                Type = UserType.Lead
            };

            var exception = new Exception("Unexpected error");
            _mockUserService.Setup(x => x.CreateUserAsync(request))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.CreateUser(request);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Error creating user");
        }

        #endregion

        #region UpdateUser Tests

        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenUserUpdatedSuccessfully()
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

            var updatedUser = new User
            {
                Id = userId,
                Name = request.Name,
                Email = request.Email,
                Password = request.Password,
                Type = request.Type,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };

            _mockUserService.Setup(x => x.UpdateUserAsync(userId, request))
                .ReturnsAsync(updatedUser);

            // Act
            var result = await _controller.UpdateUser(userId, request);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var userResponse = okResult!.Value as UserResponse;
            userResponse.Should().NotBeNull();
            userResponse!.Id.Should().Be(userId);
            userResponse.Name.Should().Be(request.Name);
            userResponse.Email.Should().Be(request.Email);
            userResponse.Type.Should().Be(request.Type);
            _mockUserService.Verify(x => x.UpdateUserAsync(userId, request), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
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

            _mockUserService.Setup(x => x.UpdateUserAsync(userId, request))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.UpdateUser(userId, request);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be($"User with ID {userId} not found");
            _mockUserService.Verify(x => x.UpdateUserAsync(userId, request), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var userId = 1;
            var request = new UpdateUserRequest
            {
                Name = "", // Invalid
                Email = "invalid-email",
                Password = "123", // Too short
                Type = UserType.Lead
            };

            _controller.ModelState.AddModelError("Name", "Name is required");
            _controller.ModelState.AddModelError("Email", "Invalid email format");
            _controller.ModelState.AddModelError("Password", "Password too short");

            // Act
            var result = await _controller.UpdateUser(userId, request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            _mockUserService.Verify(x => x.UpdateUserAsync(It.IsAny<int>(), It.IsAny<UpdateUserRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenArgumentExceptionThrown()
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

            var exception = new ArgumentException("Invalid user data");
            _mockUserService.Setup(x => x.UpdateUserAsync(userId, request))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.UpdateUser(userId, request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Invalid user data");
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnConflict_WhenInvalidOperationExceptionThrown()
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

            var exception = new InvalidOperationException("User with email 'existing@test.com' already exists");
            _mockUserService.Setup(x => x.UpdateUserAsync(userId, request))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.UpdateUser(userId, request);

            // Assert
            result.Result.Should().BeOfType<ConflictObjectResult>();
            var conflictResult = result.Result as ConflictObjectResult;
            conflictResult!.Value.Should().Be("User with email 'existing@test.com' already exists");
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnInternalServerError_WhenUnexpectedExceptionThrown()
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

            var exception = new Exception("Unexpected error");
            _mockUserService.Setup(x => x.UpdateUserAsync(userId, request))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.UpdateUser(userId, request);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Error updating user");
        }

        #endregion

        #region DeleteUser Tests

        [Fact]
        public async Task DeleteUser_ShouldReturnNoContent_WhenUserDeletedSuccessfully()
        {
            // Arrange
            var userId = 1;
            _mockUserService.Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockUserService.Verify(x => x.DeleteUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 999;
            _mockUserService.Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be($"User with ID {userId} not found");
            _mockUserService.Verify(x => x.DeleteUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnInternalServerError_WhenServiceThrowsException()
        {
            // Arrange
            var userId = 1;
            var exception = new Exception("Database error");
            _mockUserService.Setup(x => x.DeleteUserAsync(userId))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Error deleting user");
        }

        #endregion
    }
}
