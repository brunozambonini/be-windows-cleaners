namespace be_windows_cleaners.Validators
{
    public class ImageValidators : IImageValidators
    {
        public void ValidateImageInput(string title, string base64Data)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required");

            if (string.IsNullOrWhiteSpace(base64Data))
                throw new ArgumentException("Image data is required");
        }

        public void ValidateImageInput(string title, IFormFile imageFile)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required");

            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("Image file is required");
        }

        public void ValidateBase64String(string base64Data)
        {
            try
            {
                Convert.FromBase64String(base64Data);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Image data must be in valid base64 format");
            }
        }

        public void ValidateFileType(IFormFile imageFile)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new ArgumentException($"File type not allowed. Accepted types: {string.Join(", ", allowedExtensions)}");
            }
        }

        public void ValidateFileSize(IFormFile imageFile)
        {
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (imageFile.Length > maxFileSize)
            {
                throw new ArgumentException("File too large. Maximum allowed size: 10MB");
            }
        }
    }
}
