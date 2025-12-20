namespace GreenEye.Service.IService
{
    public interface IImageService
    {
        Task<GeneralResponse<string>> UploadImage(IFormFile image);
        GeneralResponse<string> RemoveImage(string imagePath);
    }
}
