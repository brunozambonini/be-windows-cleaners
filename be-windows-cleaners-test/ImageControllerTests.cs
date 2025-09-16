using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using be_windows_cleaners.Controllers;
using be_windows_cleaners.Models;
using be_windows_cleaners.Services;
using FluentAssertions;

namespace be_windows_cleaners_test
{
    public class ImageControllerTests
    {
        private readonly Mock<ILogger<ImageController>> _mockLogger;
        private readonly Mock<IImageService> _mockImageService;
        private readonly ImageController _controller;

        public ImageControllerTests()
        {
            _mockLogger = new Mock<ILogger<ImageController>>();
            _mockImageService = new Mock<IImageService>();
            _controller = new ImageController(_mockLogger.Object, _mockImageService.Object);
        }

        #region GetImages Tests

        [Fact]
        public async Task GetImages_ShouldReturnAllImages_WhenNoUserIdProvided()
        {
            // Arrange
            var expectedImages = new List<Image>
            {
                new Image { Id = 1, Title = "Image 1", ImageData = "base64data1", Created_At = DateTime.UtcNow, UserId = 1 },
                new Image { Id = 2, Title = "Image 2", ImageData = "base64data2", Created_At = DateTime.UtcNow, UserId = 2 }
            };

            _mockImageService.Setup(x => x.GetAllImagesAsync())
                .ReturnsAsync(expectedImages);

            // Act
            var result = await _controller.GetImages();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedImages);
            _mockImageService.Verify(x => x.GetAllImagesAsync(), Times.Once);
            _mockImageService.Verify(x => x.GetImagesByUserIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetImages_ShouldReturnUserImages_WhenUserIdProvided()
        {
            // Arrange
            var userId = 1;
            var expectedImages = new List<Image>
            {
                new Image { Id = 1, Title = "User Image 1", ImageData = "base64data1", Created_At = DateTime.UtcNow, UserId = userId },
                new Image { Id = 2, Title = "User Image 2", ImageData = "base64data2", Created_At = DateTime.UtcNow, UserId = userId }
            };

            _mockImageService.Setup(x => x.GetImagesByUserIdAsync(userId))
                .ReturnsAsync(expectedImages);

            // Act
            var result = await _controller.GetImages(userId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedImages);
            _mockImageService.Verify(x => x.GetImagesByUserIdAsync(userId), Times.Once);
            _mockImageService.Verify(x => x.GetAllImagesAsync(), Times.Never);
        }

        [Fact]
        public async Task GetImages_ShouldReturnInternalServerError_WhenServiceThrowsException()
        {
            // Arrange
            var exception = new Exception("Database error");
            _mockImageService.Setup(x => x.GetAllImagesAsync())
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.GetImages();

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Error retrieving images");
        }

        #endregion


        #region UploadImage Tests

        [Fact]
        public async Task UploadImage_ShouldReturnCreatedResult_WhenValidRequest()
        {
            // Arrange
            var request = new UploadImageRequest
            {
                Title = "Test Image",
                ImageFile = CreateMockFormFile("test.jpg", "image/jpeg", "fake image data"),
                UserId = 1
            };

            var expectedImage = new Image
            {
                Id = 1,
                Title = request.Title,
                ImageData = "base64data",
                Created_At = DateTime.UtcNow,
                UserId = request.UserId
            };

            _mockImageService.Setup(x => x.AddImageFromFileAsync(request.Title, request.ImageFile, request.UserId))
                .ReturnsAsync(expectedImage);

            // Act
            var result = await _controller.UploadImage(request);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be(nameof(ImageController.GetImages));
            createdResult.RouteValues!["id"].Should().Be(expectedImage.Id);
            createdResult.Value.Should().BeEquivalentTo(expectedImage);
            _mockImageService.Verify(x => x.AddImageFromFileAsync(request.Title, request.ImageFile, request.UserId), Times.Once);
        }

        [Fact]
        public async Task UploadImage_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var request = new UploadImageRequest
            {
                Title = "", // Invalid
                ImageFile = CreateMockFormFile("test.jpg", "image/jpeg", "fake image data"),
                UserId = 1
            };

            _controller.ModelState.AddModelError("Title", "Title is required");

            // Act
            var result = await _controller.UploadImage(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            _mockImageService.Verify(x => x.AddImageFromFileAsync(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task UploadImage_ShouldReturnBadRequest_WhenArgumentExceptionThrown()
        {
            // Arrange
            var request = new UploadImageRequest
            {
                Title = "Test Image",
                ImageFile = CreateMockFormFile("test.jpg", "image/jpeg", "fake image data"),
                UserId = 1
            };

            var exception = new ArgumentException("Invalid image data");
            _mockImageService.Setup(x => x.AddImageFromFileAsync(request.Title, request.ImageFile, request.UserId))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.UploadImage(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Invalid image data");
        }

        [Fact]
        public async Task UploadImage_ShouldReturnBadRequest_WhenInvalidOperationExceptionThrown()
        {
            // Arrange
            var request = new UploadImageRequest
            {
                Title = "Test Image",
                ImageFile = CreateMockFormFile("test.jpg", "image/jpeg", "fake image data"),
                UserId = 1
            };

            var exception = new InvalidOperationException("Image limit reached");
            _mockImageService.Setup(x => x.AddImageFromFileAsync(request.Title, request.ImageFile, request.UserId))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.UploadImage(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Image limit reached");
        }

        [Fact]
        public async Task UploadImage_ShouldReturnInternalServerError_WhenUnexpectedExceptionThrown()
        {
            // Arrange
            var request = new UploadImageRequest
            {
                Title = "Test Image",
                ImageFile = CreateMockFormFile("test.jpg", "image/jpeg", "fake image data"),
                UserId = 1
            };

            var exception = new Exception("Unexpected error");
            _mockImageService.Setup(x => x.AddImageFromFileAsync(request.Title, request.ImageFile, request.UserId))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.UploadImage(request);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Unexpected error");
        }

        #endregion

        #region DeleteImage Tests

        [Fact]
        public async Task DeleteImage_ShouldReturnNoContent_WhenImageDeletedSuccessfully()
        {
            // Arrange
            var imageId = 1;
            var userId = 1;

            _mockImageService.Setup(x => x.DeleteImageByUserAsync(imageId, userId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteImage(imageId, userId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockImageService.Verify(x => x.DeleteImageByUserAsync(imageId, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteImage_ShouldReturnNotFound_WhenImageDoesNotExist()
        {
            // Arrange
            var imageId = 999;
            var userId = 1;

            _mockImageService.Setup(x => x.DeleteImageByUserAsync(imageId, userId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteImage(imageId, userId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be($"Image with ID {imageId} not found or you don't have permission to delete it");
            _mockImageService.Verify(x => x.DeleteImageByUserAsync(imageId, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteImage_ShouldReturnNotFound_WhenUserDoesNotOwnImage()
        {
            // Arrange
            var imageId = 1;
            var userId = 1;

            _mockImageService.Setup(x => x.DeleteImageByUserAsync(imageId, userId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteImage(imageId, userId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be($"Image with ID {imageId} not found or you don't have permission to delete it");
            _mockImageService.Verify(x => x.DeleteImageByUserAsync(imageId, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteImage_ShouldReturnInternalServerError_WhenServiceThrowsException()
        {
            // Arrange
            var imageId = 1;
            var userId = 1;
            var exception = new Exception("Database error");

            _mockImageService.Setup(x => x.DeleteImageByUserAsync(imageId, userId))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.DeleteImage(imageId, userId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Error deleting image");
        }

        #endregion

        #region ResetDatabase Tests

        [Fact]
        public async Task ResetDatabase_ShouldReturnOk_WhenResetSuccessful()
        {
            // Arrange
            var deletedCount = 5;
            _mockImageService.Setup(x => x.DeleteAllImagesAsync())
                .ReturnsAsync(deletedCount);

            // Act
            var result = await _controller.ResetDatabase();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value;
            response.Should().NotBeNull();
            
            // Check if the response has the expected properties
            var responseType = response.GetType();
            var messageProperty = responseType.GetProperty("message");
            var deletedCountProperty = responseType.GetProperty("deletedCount");
            
            messageProperty.Should().NotBeNull();
            deletedCountProperty.Should().NotBeNull();
            
            messageProperty!.GetValue(response).Should().Be("Database reset successfully");
            deletedCountProperty!.GetValue(response).Should().Be(deletedCount);
            
            _mockImageService.Verify(x => x.DeleteAllImagesAsync(), Times.Once);
        }

        [Fact]
        public async Task ResetDatabase_ShouldReturnInternalServerError_WhenServiceThrowsException()
        {
            // Arrange
            var exception = new Exception("Database error");
            _mockImageService.Setup(x => x.DeleteAllImagesAsync())
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.ResetDatabase();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Error resetting database");
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
