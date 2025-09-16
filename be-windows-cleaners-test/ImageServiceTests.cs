using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using be_windows_cleaners.Models;
using be_windows_cleaners.Repository;
using be_windows_cleaners.Services;
using be_windows_cleaners.Validators;

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
                new Image { Id = 1, Title = "Test Image 1", ImageData = "base64data1", Created_At = DateTime.UtcNow },
                new Image { Id = 2, Title = "Test Image 2", ImageData = "base64data2", Created_At = DateTime.UtcNow }
            };

            _mockImageRepository.Setup(x => x.GetAllImagesAsync())
                .ReturnsAsync(expectedImages);

            // Act
            var result = await _imageService.GetAllImagesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal(expectedImages, result);
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
            Assert.Equal("Database error", thrownException.Message);
            _mockImageRepository.Verify(x => x.GetAllImagesAsync(), Times.Once);
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
                Created_At = DateTime.UtcNow 
            };

            _mockImageRepository.Setup(x => x.GetImageByIdAsync(imageId))
                .ReturnsAsync(expectedImage);

            // Act
            var result = await _imageService.GetImageByIdAsync(imageId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedImage, result);
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
            Assert.Null(result);
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
            Assert.Equal("Database error", thrownException.Message);
            _mockImageRepository.Verify(x => x.GetImageByIdAsync(imageId), Times.Once);
        }

        #endregion

        #region AddImageFromFileAsync Tests

        [Fact]
        public async Task AddImageFromFileAsync_ShouldReturnImage_WhenValidInput()
        {
            // Arrange
            var title = "Test Image";
            var imageFile = CreateMockFormFile("test.jpg", "image/jpeg", "fake image data");
            var expectedImage = new Image 
            { 
                Id = 1, 
                Title = title.Trim(), 
                ImageData = "ZmFrZSBpbWFnZSBkYXRh", // base64 of "fake image data"
                Created_At = DateTime.UtcNow 
            };

            _mockImageRepository.Setup(x => x.GetImageCountAsync())
                .ReturnsAsync(5); // Below limit of 10

            _mockImageRepository.Setup(x => x.AddImageAsync(It.IsAny<Image>()))
                .ReturnsAsync(expectedImage);

            // Act
            var result = await _imageService.AddImageFromFileAsync(title, imageFile);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedImage.Id, result.Id);
            Assert.Equal(title.Trim(), result.Title);
            Assert.Equal("ZmFrZSBpbWFnZSBkYXRh", result.ImageData);
            Assert.True(result.Created_At <= DateTime.UtcNow);

            _mockImageValidators.Verify(x => x.ValidateImageInput(title, imageFile), Times.Once);
            _mockImageValidators.Verify(x => x.ValidateFileType(imageFile), Times.Once);
            _mockImageValidators.Verify(x => x.ValidateFileSize(imageFile), Times.Once);
            _mockImageRepository.Verify(x => x.GetImageCountAsync(), Times.Once);
            _mockImageRepository.Verify(x => x.AddImageAsync(It.IsAny<Image>()), Times.Once);
        }

        [Fact]
        public async Task AddImageFromFileAsync_ShouldThrowInvalidOperationException_WhenImageLimitReached()
        {
            // Arrange
            var title = "Test Image";
            var imageFile = CreateMockFormFile("test.jpg", "image/jpeg", "fake image data");

            _mockImageRepository.Setup(x => x.GetImageCountAsync())
                .ReturnsAsync(10); // At limit of 10

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _imageService.AddImageFromFileAsync(title, imageFile));

            Assert.Contains("Maximum image limit reached", exception.Message);
            Assert.Contains("10", exception.Message);

            _mockImageRepository.Verify(x => x.GetImageCountAsync(), Times.Once);
            _mockImageRepository.Verify(x => x.AddImageAsync(It.IsAny<Image>()), Times.Never);
        }

        [Fact]
        public async Task AddImageFromFileAsync_ShouldThrowException_WhenValidationFails()
        {
            // Arrange
            var title = "";
            var imageFile = CreateMockFormFile("test.jpg", "image/jpeg", "fake image data");

            _mockImageValidators.Setup(x => x.ValidateImageInput(title, imageFile))
                .Throws(new ArgumentException("Title cannot be empty"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _imageService.AddImageFromFileAsync(title, imageFile));

            Assert.Equal("Title cannot be empty", exception.Message);

            _mockImageValidators.Verify(x => x.ValidateImageInput(title, imageFile), Times.Once);
            _mockImageRepository.Verify(x => x.GetImageCountAsync(), Times.Never);
            _mockImageRepository.Verify(x => x.AddImageAsync(It.IsAny<Image>()), Times.Never);
        }

        [Fact]
        public async Task AddImageFromFileAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var title = "Test Image";
            var imageFile = CreateMockFormFile("test.jpg", "image/jpeg", "fake image data");
            var exception = new Exception("Database error");

            _mockImageRepository.Setup(x => x.GetImageCountAsync())
                .ReturnsAsync(5);

            _mockImageRepository.Setup(x => x.AddImageAsync(It.IsAny<Image>()))
                .ThrowsAsync(exception);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<Exception>(() => 
                _imageService.AddImageFromFileAsync(title, imageFile));

            Assert.Equal("Database error", thrownException.Message);

            _mockImageRepository.Verify(x => x.GetImageCountAsync(), Times.Once);
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
            Assert.True(result);
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
            Assert.False(result);
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
            Assert.Equal("Database error", thrownException.Message);
            _mockImageRepository.Verify(x => x.DeleteImageAsync(imageId), Times.Once);
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
