namespace be_windows_cleaners.Validators
{
    public interface IImageValidators
    {
        void ValidateImageInput(string title, string base64Data);
        void ValidateImageInput(string title, IFormFile imageFile);
        void ValidateBase64String(string base64Data);
        void ValidateFileType(IFormFile imageFile);
        void ValidateFileSize(IFormFile imageFile);
    }
}
