using GreenEye.Dto;
using GreenEye.Dto.Authentication;

namespace GreenEye.Service.IService
{
    public interface IAuthenticationService
    {
        Task<GeneralResponse<string>> RegisterAsync(RegisterDto model);

        Task<GeneralResponse<string>> VerifyOTP(VerifyOtpDto verifyOtpDto);

        Task<GeneralResponse<string>> CreateUserAsync();

        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

        Task<GeneralResponse<string>> ForgetPassword(string email);

        Task<GeneralResponse<string>> Login(LoginDto loginDTO);

        Task<GeneralResponse<string>> ResendOtpAsync(ResendOtpDto resendOtpDto);
    }
}
