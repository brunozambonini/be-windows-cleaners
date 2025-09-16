using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using be_windows_cleaners.Models;
using be_windows_cleaners.Repository;
using be_windows_cleaners.Services;
using be_windows_cleaners.Validators;
using FluentAssertions;

namespace be_windows_cleaners_test
{
    public class ImageServiceTests
    {
        private readonly Mock<IImageRepository> _mockImageRepository;
        private readonly Mock<IImageValidators> _mockImageValidators;
        private readonly Mock<ILogger<ImageService>> _mockLogger;
        private readonly ImageService _imageService;

        public ImageServiceTests()
        {
            _mockImageRepository = new Mock<IImageRepository>();
            _mockImageValidators = new Mock<IImageValidators>();
            _mockLogger = new Mock<ILogger<ImageService>>();
            _imageService = new ImageService(_mockImageRepository.Object, _mockImageValidators.Object, _mockLogger.Object);
        }

        #region GetAllImagesAsync Tests

        [Fact]
        public async Task GetAllImagesAsync_ShouldReturnImages_WhenRepositoryReturnsData()
        {
            // Arrange
            var expectedImages = new List<Image>
            {
                new Image { Id = 1, Title = "Test Image 1", ImageData = "base64data1", Created_At = DateTime.UtcNow, UserId = 1 },
                new Image { Id = 2, Title = "Test Image 2", ImageData = "base64data2", Created_At = DateTime.UtcNow, UserId = 2 }
            };

            _mockImageRepository.Setup(x => x.GetAllImagesAsync())
                .ReturnsAsync(expectedImages);

            // Act
            var result = await _imageService.GetAllImagesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedImages);
            _mockImageRepository.Verify(x => x.GetAllImagesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllImagesAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var exception = new Exception("Database error");
            _mockImageRepository.Setup(x => x.GetAllImagesAsync())
                .ThrowsAsync(exception);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<Exception>(() => _imageService.GetAllImagesAsync());
            thrownException.Message.Should().Be("Database error");
            _mockImageRepository.Verify(x => x.GetAllImagesAsync(), Times.Once);
        }

        #endregion

        #region GetImagesByUserIdAsync Tests

        [Fact]
        public async Task GetImagesByUserIdAsync_ShouldReturnUserImages_WhenUserHasImages()
        {
            // Arrange
            var userId = 1;
            var expectedImages = new List<Image>
            {
                new Image { Id = 1, Title = "User Image 1", ImageData = "base64data1", Created_At = DateTime.UtcNow, UserId = userId },
                new Image { Id = 2, Title = "User Image 2", ImageData = "base64data2", Created_At = DateTime.UtcNow, UserId = userId }
            };

            _mockImageRepository.Setup(x => x.GetImagesByUserIdAsync(userId))
                .ReturnsAsync(expectedImages);

            // Act
            var result = await _imageService.GetImagesByUserIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedImages);
            _mockImageRepository.Verify(x => x.GetImagesByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetImagesByUserIdAsync_ShouldReturnEmptyList_WhenUserHasNoImages()
        {
            // Arrange
            var userId = 1;
            var expectedImages = new List<Image>();

            _mockImageRepository.Setup(x => x.GetImagesByUserIdAsync(userId))
                .ReturnsAsync(expectedImages);

            // Act
            var result = await _imageService.GetImagesByUserIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockImageRepository.Verify(x => x.GetImagesByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetImagesByUserIdAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var userId = 1;
            var exception = new Exception("Database error");
            _mockImageRepository.Setup(x => x.GetImagesByUserIdAsync(userId))
                .ThrowsAsync(exception);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<Exception>(() => _imageService.GetImagesByUserIdAsync(userId));
            thrownException.Message.Should().Be("Database error");
            _mockImageRepository.Verify(x => x.GetImagesByUserIdAsync(userId), Times.Once);
        }

        #endregion

        #region GetImageByIdAsync Tests

        [Fact]
        public async Task GetImageByIdAsync_ShouldReturnImage_WhenImageExists()
        {
            // Arrange
            var imageId = 1;
            var expectedImage = new Image 
            { 
                Id = imageId, 
                Title = "Test Image", 
                ImageData = "base64data", 
                Created_At = DateTime.UtcNow,
                UserId = 1
            };

            _mockImageRepository.Setup(x => x.GetImageByIdAsync(imageId))
                .ReturnsAsync(expectedImage);

            // Act
            var result = await _imageService.GetImageByIdAsync(imageId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedImage);
            _mockImageRepository.Verify(x => x.GetImageByIdAsync(imageId), Times.Once);
        }

        [Fact]
        public async Task GetImageByIdAsync_ShouldReturnNull_WhenImageDoesNotExist()
        {
            // Arrange
            var imageId = 999;
            _mockImageRepository.Setup(x => x.GetImageByIdAsync(imageId))
                .ReturnsAsync((Image?)null);

            // Act
            var result = await _imageService.GetImageByIdAsync(imageId);

            // Assert
            result.Should().BeNull();
            _mockImageRepository.Verify(x => x.GetImageByIdAsync(imageId), Times.Once);
        }

        [Fact]
        public async Task GetImageByIdAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var imageId = 1;
            var exception = new Exception("Database error");
            _mockImageRepository.Setup(x => x.GetImageByIdAsync(imageId))
                .ThrowsAsync(exception);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<Exception>(() => _imageService.GetImageByIdAsync(imageId));
            thrownException.Message.Should().Be("Database error");
            _mockImageRepository.Verify(x => x.GetImageByIdAsync(imageId), Times.Once);
        }

        #endregion

        #region AddImageFromFileAsync Tests

        [Fact]
        public async Task AddImageFromFileAsync_ShouldReturnImage_WhenValidInput()
        {
            // Arrange
            var title = "Test Image";
            var userId = 1;
            var imageFile = CreateMockFormFile("test.jpg", "image/jpeg", "fake image data");
            var expectedImage = new Image 
            { 
                Id = 1, 
                Title = title.Trim(), 
                ImageData = "ZmFrZSBpbWFnZSBkYXRh", // base64 of "fake image data"
                Created_At = DateTime.UtcNow,
                UserId = userId
            };

            _mockImageRepository.Setup(x => x.GetImageCountAsync(userId))
                .ReturnsAsync(5); // Below limit of 10

            _mockImageRepository.Setup(x => x.AddImageAsync(It.IsAny<Image>()))
                .ReturnsAsync(expectedImage);

            // Act
            var result = await _imageService.AddImageFromFileAsync(title, imageFile, userId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(expectedImage.Id);
            result.Title.Should().Be(title.Trim());
            result.ImageData.Should().Be("ZmFrZSBpbWFnZSBkYXRh");
            result.UserId.Should().Be(userId);
            result.Created_At.Should().BeOnOrBefore(DateTime.UtcNow);

            _mockImageValidators.Verify(x => x.ValidateImageInput(title, imageFile), Times.Once);
            _mockImageValidators.Verify(x => x.ValidateFileType(imageFile), Times.Once);
            _mockImageValidators.Verify(x => x.ValidateFileSize(imageFile), Times.Once);
            _mockImageRepository.Verify(x => x.GetImageCountAsync(userId), Times.Once);
            _mockImageRepository.Verify(x => x.AddImageAsync(It.IsAny<Image>()), Times.Once);
        }

        [Fact]
        public async Task AddImageFromFileAsync_ShouldThrowInvalidOperationException_WhenImageLimitReached()
        {
            // Arrange
            var title = "Test Image";
            var userId = 1;
            var imageFile = CreateMockFormFile("test.jpg", "image/jpeg", "fake image data");

            _mockImageRepository.Setup(x => x.GetImageCountAsync(userId))
                .ReturnsAsync(10); // At limit of 10

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _imageService.AddImageFromFileAsync(title, imageFile, userId));

            exception.Message.Should().Contain("Maximum image limit reached");
            exception.Message.Should().Contain("10");

            _mockImageRepository.Verify(x => x.GetImageCountAsync(userId), Times.Once);
            _mockImageRepository.Verify(x => x.AddImageAsync(It.IsAny<Image>()), Times.Never);
        }

        [Fact]
        public async Task AddImageFromFileAsync_ShouldThrowException_WhenValidationFails()
        {
            // Arrange
            var title = "";
            var userId = 1;
            var imageFile = CreateMockFormFile("test.jpg", "image/jpeg", "fake image data");

            _mockImageValidators.Setup(x => x.ValidateImageInput(title, imageFile))
                .Throws(new ArgumentException("Title cannot be empty"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _imageService.AddImageFromFileAsync(title, imageFile, userId));

            exception.Message.Should().Be("Title cannot be empty");

            _mockImageValidators.Verify(x => x.ValidateImageInput(title, imageFile), Times.Once);
            _mockImageRepository.Verify(x => x.GetImageCountAsync(It.IsAny<int>()), Times.Never);
            _mockImageRepository.Verify(x => x.AddImageAsync(It.IsAny<Image>()), Times.Never);
        }

        [Fact]
        public async Task AddImageFromFileAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var title = "Test Image";
            var userId = 1;
            var imageFile = CreateMockFormFile("test.jpg", "image/jpeg", "fake image data");
            var exception = new Exception("Database error");

            _mockImageRepository.Setup(x => x.GetImageCountAsync(userId))
                .ReturnsAsync(5);

            _mockImageRepository.Setup(x => x.AddImageAsync(It.IsAny<Image>()))
                .ThrowsAsync(exception);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<Exception>(() => 
                _imageService.AddImageFromFileAsync(title, imageFile, userId));

            thrownException.Message.Should().Be("Database error");

            _mockImageRepository.Verify(x => x.GetImageCountAsync(userId), Times.Once);
            _mockImageRepository.Verify(x => x.AddImageAsync(It.IsAny<Image>()), Times.Once);
        }

        #endregion

        #region DeleteImageAsync Tests

        [Fact]
        public async Task DeleteImageAsync_ShouldReturnTrue_WhenImageExists()
        {
            // Arrange
            var imageId = 1;
            _mockImageRepository.Setup(x => x.DeleteImageAsync(imageId))
                .ReturnsAsync(true);

            // Act
            var result = await _imageService.DeleteImageAsync(imageId);

            // Assert
            result.Should().BeTrue();
            _mockImageRepository.Verify(x => x.DeleteImageAsync(imageId), Times.Once);
        }

        [Fact]
        public async Task DeleteImageAsync_ShouldReturnFalse_WhenImageDoesNotExist()
        {
            // Arrange
            var imageId = 999;
            _mockImageRepository.Setup(x => x.DeleteImageAsync(imageId))
                .ReturnsAsync(false);

            // Act
            var result = await _imageService.DeleteImageAsync(imageId);

            // Assert
            result.Should().BeFalse();
            _mockImageRepository.Verify(x => x.DeleteImageAsync(imageId), Times.Once);
        }

        [Fact]
        public async Task DeleteImageAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var imageId = 1;
            var exception = new Exception("Database error");
            _mockImageRepository.Setup(x => x.DeleteImageAsync(imageId))
                .ThrowsAsync(exception);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<Exception>(() => _imageService.DeleteImageAsync(imageId));
            thrownException.Message.Should().Be("Database error");
            _mockImageRepository.Verify(x => x.DeleteImageAsync(imageId), Times.Once);
        }

        #endregion

        #region DeleteImageByUserAsync Tests

        [Fact]
        public async Task DeleteImageByUserAsync_ShouldReturnTrue_WhenUserOwnsImage()
        {
            // Arrange
            var imageId = 1;
            var userId = 1;
            var image = new Image { Id = imageId, UserId = userId, Title = "Test Image", ImageData = "base64data" };

            _mockImageRepository.Setup(x => x.GetImageByIdAsync(imageId))
                .ReturnsAsync(image);
            _mockImageRepository.Setup(x => x.DeleteImageAsync(imageId))
                .ReturnsAsync(true);

            // Act
            var result = await _imageService.DeleteImageByUserAsync(imageId, userId);

            // Assert
            result.Should().BeTrue();
            _mockImageRepository.Verify(x => x.GetImageByIdAsync(imageId), Times.Once);
            _mockImageRepository.Verify(x => x.DeleteImageAsync(imageId), Times.Once);
        }

        [Fact]
        public async Task DeleteImageByUserAsync_ShouldReturnFalse_WhenImageDoesNotExist()
        {
            // Arrange
            var imageId = 999;
            var userId = 1;

            _mockImageRepository.Setup(x => x.GetImageByIdAsync(imageId))
                .ReturnsAsync((Image?)null);

            // Act
            var result = await _imageService.DeleteImageByUserAsync(imageId, userId);

            // Assert
            result.Should().BeFalse();
            _mockImageRepository.Verify(x => x.GetImageByIdAsync(imageId), Times.Once);
            _mockImageRepository.Verify(x => x.DeleteImageAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteImageByUserAsync_ShouldReturnFalse_WhenUserDoesNotOwnImage()
        {
            // Arrange
            var imageId = 1;
            var userId = 1;
            var imageOwnerId = 2;
            var image = new Image { Id = imageId, UserId = imageOwnerId, Title = "Test Image", ImageData = "base64data" };

            _mockImageRepository.Setup(x => x.GetImageByIdAsync(imageId))
                .ReturnsAsync(image);

            // Act
            var result = await _imageService.DeleteImageByUserAsync(imageId, userId);

            // Assert
            result.Should().BeFalse();
            _mockImageRepository.Verify(x => x.GetImageByIdAsync(imageId), Times.Once);
            _mockImageRepository.Verify(x => x.DeleteImageAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteImageByUserAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var imageId = 1;
            var userId = 1;
            var exception = new Exception("Database error");

            _mockImageRepository.Setup(x => x.GetImageByIdAsync(imageId))
                .ThrowsAsync(exception);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<Exception>(() => _imageService.DeleteImageByUserAsync(imageId, userId));
            thrownException.Message.Should().Be("Database error");
            _mockImageRepository.Verify(x => x.GetImageByIdAsync(imageId), Times.Once);
        }

        #endregion

        #region DeleteAllImagesAsync Tests

        [Fact]
        public async Task DeleteAllImagesAsync_ShouldReturnDeletedCount_WhenImagesExist()
        {
            // Arrange
            var expectedCount = 5;
            _mockImageRepository.Setup(x => x.DeleteAllImagesAsync())
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _imageService.DeleteAllImagesAsync();

            // Assert
            result.Should().Be(expectedCount);
            _mockImageRepository.Verify(x => x.DeleteAllImagesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAllImagesAsync_ShouldReturnZero_WhenNoImagesExist()
        {
            // Arrange
            var expectedCount = 0;
            _mockImageRepository.Setup(x => x.DeleteAllImagesAsync())
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _imageService.DeleteAllImagesAsync();

            // Assert
            result.Should().Be(expectedCount);
            _mockImageRepository.Verify(x => x.DeleteAllImagesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAllImagesAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var exception = new Exception("Database error");
            _mockImageRepository.Setup(x => x.DeleteAllImagesAsync())
                .ThrowsAsync(exception);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<Exception>(() => _imageService.DeleteAllImagesAsync());
            thrownException.Message.Should().Be("Database error");
            _mockImageRepository.Verify(x => x.DeleteAllImagesAsync(), Times.Once);
        }

        #endregion

        #region Helper Methods

        private static IFormFile CreateMockFormFile(string fileName, string contentType, string content)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.FileName).Returns(fileName);
            formFile.Setup(f => f.ContentType).Returns(contentType);
            formFile.Setup(f => f.Length).Returns(bytes.Length);
            formFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream stream, CancellationToken token) => stream.WriteAsync(bytes, 0, bytes.Length, token));

            return formFile.Object;
        }

        #endregion
    }
}