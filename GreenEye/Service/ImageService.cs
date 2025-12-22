using GreenEye.Dto.Responses;

namespace GreenEye.Service
{
    public class ImageService(IWebHostEnvironment _webHost, IHttpContextAccessor _httpContext) : IImageService
    {
        // Convert image name to url( يقدر يتعامل معها ويظهرها client side علشان ال )
        private string GenerateFileUrl(string fileName)
        {
            var request = _httpContext.HttpContext?.Request;
            if (request == null)
                return string.Empty;

            return $"{request?.Scheme}://{request?.Host}/images/{fileName}";
        }

        public async Task<GeneralResponse<string>> UploadImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return new GeneralResponse<string> { IsSuccess = false, Message = "There is no image" };

            // Check for extension
            var allowExtensions = new[] { ".jpg", ".png", ".jpeg" };
            var extension = Path.GetExtension(image.FileName).ToLower();

            if (!allowExtensions.Contains(extension))
                return new GeneralResponse<string> { IsSuccess = false, Message = "Invalid file type" };

            var uploadFolder = Path.Combine(_webHost.WebRootPath, "images");
            // if folder not found create this folder
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";

            var fullPath = Path.Combine(uploadFolder, extension);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            return new GeneralResponse<string> { IsSuccess = true, Message = "Image Uploaded!", Data = GenerateFileUrl(fileName)};
        }

        public GeneralResponse<string> RemoveImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return new GeneralResponse<string> { IsSuccess = false, Message = "There is no image" };

                // AbsolutePath => images/fileName
                var fileName = Path.GetFileName(new Uri(imagePath).AbsolutePath);
                var filePath = Path.Combine(_webHost.WebRootPath, "images", fileName);


                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return new GeneralResponse<string> { IsSuccess = true, Message = "Image deleted!" };
                }
                return new GeneralResponse<string> { IsSuccess = false, Message = "File already not found" };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<string> { IsSuccess = false, Message = ex.Message };
            } 
        }
    }
}
