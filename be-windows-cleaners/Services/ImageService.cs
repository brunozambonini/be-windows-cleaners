using be_windows_cleaners.Models;
using be_windows_cleaners.Repository;
using be_windows_cleaners.Validators;

namespace be_windows_cleaners.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _imageRepository;
        private readonly IImageValidators _imageValidators;
        private readonly ILogger<ImageService> _logger;
        private readonly int MAX_IMAGES = 10;
        
        public ImageService(IImageRepository imageRepository, IImageValidators imageValidators, ILogger<ImageService> logger)
        {
            _imageRepository = imageRepository;
            _imageValidators = imageValidators;
            _logger = logger;
        }

        public async Task<IEnumerable<Image>> GetAllImagesAsync()
        {
            try
            {
                return await _imageRepository.GetAllImagesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all images");
                throw;
            }
        }

        public async Task<IEnumerable<Image>> GetImagesByUserIdAsync(int userId)
        {
            try
            {
                return await _imageRepository.GetImagesByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving images for user ID {UserId}", userId);
                throw;
            }
        }

        public async Task<Image> AddImageFromFileAsync(string title, IFormFile imageFile, int userId)
        {
            try
            {
                // Validate image
                ValidateImage(title, imageFile);

                // Check image limit
                await CheckImageLimitAsync(userId);
                
                // Convert file to base64
                string base64String = await ConvertFileToBase64Async(imageFile);

                var image = new Image
                {
                    Title = title.Trim(),
                    ImageData = base64String,
                    Created_At = DateTime.UtcNow,
                    UserId = userId
                };

                var result = await _imageRepository.AddImageAsync(image);
                _logger.LogInformation("New image uploaded with ID {Id}, title {Title} for user {UserId}", result.Id, result.Title, userId);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeleteImageByUserAsync(int id, int userId)
        {
            try
            {
                // Check if image exists
                var imageExists = await _imageRepository.ImageExistsAsync(id);
                
                if (!imageExists)
                {
                    _logger.LogWarning("Attempt to delete non-existent image with ID {Id}", id);
                    return false;
                }

                // Try to delete the image - the repository will handle ownership verification
                var result = await _imageRepository.DeleteImageAsync(id, userId);
                
                if (result)
                {
                    _logger.LogInformation("Image with ID {Id} was deleted by user {UserId}", id, userId);
                }
                else
                {
                    _logger.LogWarning("User {UserId} attempted to delete image {ImageId} but deletion failed", userId, id);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image with ID {Id} for user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<int> DeleteAllImagesAsync()
        {
            try
            {
                var deletedCount = await _imageRepository.DeleteAllImagesAsync();
                _logger.LogInformation("Database reset: {Count} images were deleted", deletedCount);
                return deletedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting database");
                throw;
            }
        }

        #region Private Methods

        private async Task<string> ConvertFileToBase64Async(IFormFile imageFile)
        {
            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            byte[] imageBytes = memoryStream.ToArray();
            return Convert.ToBase64String(imageBytes);
        }

        private void ValidateImage(string title, IFormFile imageFile)
        {
            // Validate input
            _imageValidators.ValidateImageInput(title, imageFile);

            // Validate file type
            _imageValidators.ValidateFileType(imageFile);

            // Validate file size
            _imageValidators.ValidateFileSize(imageFile);
        }

        private async Task CheckImageLimitAsync(int userId)
        {
            var currentCount = await _imageRepository.GetImageCountAsync(userId);
            
            if (currentCount >= MAX_IMAGES)
            {
                throw new InvalidOperationException($"Maximum image limit reached. Cannot add more than {MAX_IMAGES} images.");
            }
        }

        #endregion
    }
}
