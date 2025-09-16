using be_windows_cleaners.Models;

namespace be_windows_cleaners.Services
{
    public interface IImageService
    {
        Task<IEnumerable<Image>> GetAllImagesAsync();
        Task<IEnumerable<Image>> GetImagesByUserIdAsync(int userId);
        Task<Image> AddImageFromFileAsync(string title, IFormFile imageFile, int userId);
        Task<bool> DeleteImageByUserAsync(int id, int userId);
        Task<int> DeleteAllImagesAsync();
    }
}
