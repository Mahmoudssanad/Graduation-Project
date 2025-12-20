using GreenEye.Dto.Responses;

namespace GreenEye.Service.IService
{
    public interface IAuthenticationService
    {
        Task<GeneralResponse<string>> RegisterAsync(RegisterDto model);

        Task<GeneralResponse<AuthResponse>> VerifyOTP(VerifyOtpDto verifyOtpDto);

        Task<GeneralResponse<AuthResponse>> CreateUserAsync();

        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

        Task<GeneralResponse<string>> ForgetPassword(string email);

        Task<GeneralResponse<AuthResponse>> Login(LoginDto loginDTO);

        Task<GeneralResponse<string>> ResendOtpAsync(ResendOtpDto resendOtpDto);

        Task<GeneralResponse<string>> RevokeTokenAsync(string token);

        Task<GeneralResponse<AuthResponse>> RefreshTokenAsync(string token);
    }
}
