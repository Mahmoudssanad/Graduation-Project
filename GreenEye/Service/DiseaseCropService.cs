using GreenEye.Dto.Disease;
using GreenEye.Dto.Responses;
using System.Text;

namespace GreenEye.Service
{
    public class CropDiseaseService(IImageService _imageService, HttpClient _httpClient, IConfiguration _config, AppDbContext _context) : ICropDiseaseService
    {
        public async Task<GeneralResponse<CropDiseaseModelResponseDto>> GetDiseaseFromModelByImage(IFormFile image)
        {
            // Bytes الي مجموعه من ال IFormFile بنعمل استريمينج علشان نحول الصوره من 
            // MemoryStream => مكان بنخزن فيه بيانات الصوره في الميموري علي هيئه اصفار ووحايد 
            using var memoryStream = new MemoryStream();
            // memoryStream بننقل محتوي الصوره داخل ال 
            await image.CopyToAsync(memoryStream);
            // واقف في الاخر cursor للاول لانه بعد ما حطيت البيانات جوا الستريمنج بيفضل ال cursor بنرجع ال 
            // فلازم نرجعه تاني للاول علشان لما نبعت للموديل يقرئه من الاول
            memoryStream.Position = 0;

            // بنحول الصوره للطريقه اللي الموديل بيستقبل بيها الصوره علشان نبعتها في البرامتر 
            using var form = new MultipartFormDataContent();
            form.Add(new ByteArrayContent(memoryStream.ToArray()), "file", image.FileName);

            // call model
            var response = await _httpClient.PostAsync(_config["ExternalApis:CropDiseaseModelApi"], form);

            if (!response.IsSuccessStatusCode)
                return new GeneralResponse<CropDiseaseModelResponseDto> { IsSuccess = false, Message = $"Error occurred for calling model API, StatusCode: {response.StatusCode}" };

            // upload image
            var imagePath = await _imageService.UploadImage(image);
            var content = await response.Content.ReadAsStringAsync();

            // CropDiseaseModelResponseDto من object الي content لل Deserialize هنعمل 
            var result = JsonSerializer.Deserialize<CropDiseaseModelResponseDto>(content);

            // Save in database
            var cropDisease = new CropDiseaseHistory
            {
                //ImageUrl = imagePath.Data,
                Cause = result!.Cause,
                PeakSeason = result.PeakSeason,
                PredicatedDisease = result.PredicatedDisease,
                Remedy = result.Remedy,
                SentAt = DateTime.Now,
                Confidence = result.Confidence
            };
            await _context.CropDiseaseHistories.AddAsync(cropDisease);
            await _context.SaveChangesAsync();

            return new GeneralResponse<CropDiseaseModelResponseDto> { IsSuccess = true, Data = result };

        }
    }
}
