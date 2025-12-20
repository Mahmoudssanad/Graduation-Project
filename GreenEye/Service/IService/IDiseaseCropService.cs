using GreenEye.Dto.Disease;

namespace GreenEye.Service.IService
{
    public interface ICropDiseaseService
    {
        Task<GeneralResponse<CropDiseaseModelResponseDto>> GetDiseaseFromModelByImage(IFormFile image, string userId);
        Task<GeneralResponse<List<CropDiseaseHistoryDto>>> GetUserHistoryAsync(string userId);
        Task<GeneralResponse<bool>> DeleteHistoryAsync(int historyId, string userId);
    }
}
