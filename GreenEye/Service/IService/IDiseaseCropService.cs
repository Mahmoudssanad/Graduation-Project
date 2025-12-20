using GreenEye.Dto.Disease;
using GreenEye.Dto.Responses;

namespace GreenEye.Service.IService
{
    public interface ICropDiseaseService
    {
        Task<GeneralResponse<CropDiseaseModelResponseDto>> GetDiseaseFromModelByImage(IFormFile image);
    }
}
