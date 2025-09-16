using Microsoft.AspNetCore.Mvc;
using be_windows_cleaners.Models;
using be_windows_cleaners.Services;

namespace be_windows_cleaners.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ILogger<ImageController> _logger;
        private readonly IImageService _imageService;

        public ImageController(ILogger<ImageController> logger, IImageService imageService)
        {
            _logger = logger;
            _imageService = imageService;
        }

        /// <summary>
        /// Lists all images or images by user ID
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Image>>> GetImages([FromQuery] int? userId = null)
        {
            try
            {
                IEnumerable<Image> images;
                
                if (userId.HasValue)
                {
                    images = await _imageService.GetImagesByUserIdAsync(userId.Value);
                }
                else
                {
                    images = await _imageService.GetAllImagesAsync();
                }
                
                return Ok(images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving images for user ID {UserId}", userId);
                return StatusCode(500, "Error retrieving images");
            }
        }

        /// <summary>
        /// Gets a specific image by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Image>> GetImage(int id)
        {
            try
            {
                var image = await _imageService.GetImageByIdAsync(id);

                if (image == null)
                {
                    return NotFound($"Image with ID {id} not found");
                }

                return Ok(image);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving image with ID {Id}", id);
                return StatusCode(500, "Error retrieving image");
            }
        }

        /// <summary>
        /// Adds a new image via file upload
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<Image>> UploadImage([FromForm] UploadImageRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var image = await _imageService.AddImageFromFileAsync(request.Title, request.ImageFile, request.UserId);
                return CreatedAtAction(nameof(GetImage), new { id = image.Id }, image);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for user {UserId}", request.UserId);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Deletes an image by ID (requires userId to verify ownership)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id, [FromQuery] int userId)
        {
            try
            {
                var deleted = await _imageService.DeleteImageByUserAsync(id, userId);

                if (!deleted)
                {
                    // Check if image exists to provide appropriate error message
                    var image = await _imageService.GetImageByIdAsync(id);
                    if (image == null)
                    {
                        return NotFound($"Image with ID {id} not found");
                    }
                    else
                    {
                        return Forbid($"You don't have permission to delete this image. Image belongs to user {image.UserId}");
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image with ID {Id} for user {UserId}", id, userId);
                return StatusCode(500, "Error deleting image");
            }
        }

        /// <summary>
        /// Resets the database by deleting all images
        /// </summary>
        [HttpDelete("reset-db")]
        public async Task<IActionResult> ResetDatabase()
        {
            try
            {
                var deletedCount = await _imageService.DeleteAllImagesAsync();
                
                return Ok(new { 
                    message = "Database reset successfully", 
                    deletedCount = deletedCount 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting database");
                return StatusCode(500, "Error resetting database");
            }
        }
    }

    /// <summary>
    /// Model for base64 image creation request
    /// </summary>
    public class CreateImageRequest
    {
        public string Title { get; set; } = string.Empty;
        public string ImageData { get; set; } = string.Empty; // Base64 string
    }

    /// <summary>
    /// Model for file upload image request
    /// </summary>
    public class UploadImageRequest
    {
        public string Title { get; set; } = string.Empty;
        public IFormFile ImageFile { get; set; } = null!;
        public int UserId { get; set; }
    }
}
