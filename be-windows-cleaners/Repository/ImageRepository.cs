using Microsoft.EntityFrameworkCore;
using be_windows_cleaners.Data;
using be_windows_cleaners.Models;

namespace be_windows_cleaners.Repository
{
    public class ImageRepository : IImageRepository
    {
        private readonly ApplicationDbContext _context;

        public ImageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Image>> GetAllImagesAsync()
        {
            return await _context.Images
                .OrderByDescending(i => i.Created_At)
                .ToListAsync();
        }

        public async Task<IEnumerable<Image>> GetImagesByUserIdAsync(int userId)
        {
            return await _context.Images
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.Created_At)
                .ToListAsync();
        }


        public async Task<Image> AddImageAsync(Image image)
        {
            _context.Images.Add(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task<bool> DeleteImageAsync(int id, int userId)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null)
                return false;

            // Check if the user owns the image
            if (image.UserId != userId)
                return false;

            _context.Images.Remove(image);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ImageExistsAsync(int id)
        {
            return await _context.Images.AnyAsync(i => i.Id == id);
        }

        public async Task<int> GetImageCountAsync(int userId)
        {
            return await _context.Images.Where(x => x.UserId == userId).CountAsync();
        }

        public async Task<int> DeleteAllImagesAsync()
        {
            var images = await _context.Images.ToListAsync();
            int deletedCount = images.Count;
            
            if (deletedCount > 0)
            {
                _context.Images.RemoveRange(images);
                await _context.SaveChangesAsync();
            }
            
            return deletedCount;
        }
    }
}
