using GreenEye.Dto;
using GreenEye.Dto.Authentication;
using GreenEye.Enums;

namespace GreenEye.Service.IService
{
    public interface IOtpService
    {
        Task GenerateAndSendOtp(string email, OtpType type);
        Task<GeneralResponse<string>> ValidateOtp(VerifyOtpDto model);
    }
}
