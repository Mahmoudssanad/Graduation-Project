using GreenEye.Dto.Disease;
using System.Text;

namespace GreenEye.Service
{
    public class CropDiseaseService(IImageService _imageService, HttpClient _httpClient, IConfiguration _config, AppDbContext _context) : ICropDiseaseService
    {
        public async Task<GeneralResponse<CropDiseaseModelResponseDto>> GetDiseaseFromModelByImage(IFormFile image, string userId)
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

            // Deserialize response
            var content = await response.Content.ReadAsStringAsync(); 
            // CropDiseaseModelResponseDto من object الي content لل Deserialize هنعمل 
            var result = JsonSerializer.Deserialize<CropDiseaseModelResponseDto>(content);

            // Save in database
            var cropDisease = new CropDiseaseHistory
            {
                ////ImageUrl = imagePath.Data,
                //Cause = result!.Cause,
                //PeakSeason = result.PeakSeason,
                //PredicatedDisease = result.PredicatedDisease,
                //Remedy = result.Remedy,
                //SentAt = DateTime.Now,
                //Confidence = result.Confidence
                UserId = userId,                 // من الـ Controller
                ImageUrl = imagePath.Data,       // الصورة عشان تظهر في History

                PredicatedDisease = result!.PredicatedDisease!,
                Cause = result.Cause!,
                PeakSeason = result.PeakSeason!,
                Remedy = result.Remedy!,

                Confidence = double.Parse(result.Confidence!), // تحويل string لـ double
                SentAt = DateTime.Now,
                IsDeleted = false                   // جاهز للـ Soft Delete
            };
            await _context.CropDiseaseHistories.AddAsync(cropDisease);
            await _context.SaveChangesAsync();

            return new GeneralResponse<CropDiseaseModelResponseDto> { IsSuccess = true, Data = result };

        }



        public async Task<GeneralResponse<List<CropDiseaseHistoryDto>>> GetUserHistoryAsync(string userId)
        {
            try
            {
                var history = await _context.CropDiseaseHistories
                    .Where(h => h.UserId == userId && !h.IsDeleted) // فقط الـ User الحالي واللي مش متمسح
                    .OrderByDescending(h => h.SentAt)             // أحدث أولاً
                    .Select(h => new CropDiseaseHistoryDto       // تحويل للـ DTO للعرض
                    {
                        Id = h.Id,
                        ImageUrl = h.ImageUrl,
                        PredicatedDisease = h.PredicatedDisease,
                        Cause = h.Cause,
                        PeakSeason = h.PeakSeason,
                        Remedy = h.Remedy,
                        Confidence = h.Confidence,
                        SentAt = h.SentAt
                    })
                    .ToListAsync();

                return new GeneralResponse<List<CropDiseaseHistoryDto>>
                {
                    IsSuccess = true,
                    Data = history
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<List<CropDiseaseHistoryDto>>
                {
                    IsSuccess = false,
                    Message = $"Error fetching history: {ex.Message}"
                };
            }
        }


        public async Task<GeneralResponse<bool>> DeleteHistoryAsync(int historyId, string userId)
        {
            try
            {
                // نجيب العنصر من الداتابيز
                var history = await _context.CropDiseaseHistories
                    .FirstOrDefaultAsync(h => h.Id == historyId && h.UserId == userId);

                if (history == null)
                {
                    return new GeneralResponse<bool>
                    {
                        IsSuccess = false,
                        Message = "History item not found or does not belong to the user."
                    };
                }

                // Soft Delete
                history.IsDeleted = true;
                _context.CropDiseaseHistories.Update(history);
                await _context.SaveChangesAsync();

                return new GeneralResponse<bool>
                {
                    IsSuccess = true,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<bool>
                {
                    IsSuccess = false,
                    Message = $"Error deleting history: {ex.Message}"
                };
            }
        }



    }
}
