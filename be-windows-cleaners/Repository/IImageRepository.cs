using be_windows_cleaners.Models;

namespace be_windows_cleaners.Repository
{
    public interface IImageRepository
    {
        Task<IEnumerable<Image>> GetAllImagesAsync();
        Task<IEnumerable<Image>> GetImagesByUserIdAsync(int userId);
        Task<Image?> GetImageByIdAsync(int id);
        Task<Image> AddImageAsync(Image image);
        Task<bool> DeleteImageAsync(int id);
        Task<bool> ImageExistsAsync(int id);
        Task<int> GetImageCountAsync(int userId);
        Task<int> DeleteAllImagesAsync();
    }
}
