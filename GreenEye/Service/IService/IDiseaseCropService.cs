using GreenEye.Dto.Disease;

namespace GreenEye.Service.IService
{
    public interface ICropDiseaseService
    {
        Task<GeneralResponse<CropDiseaseModelResponseDto>> GetDiseaseFromModelByImage(IFormFile image);
    }
}
