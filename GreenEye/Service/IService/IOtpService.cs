namespace GreenEye.Service.IService
{
    public interface IOtpService
    {
        Task GenerateAndSendOtp(string email, OtpType type);
        Task<GeneralResponse<string>> ValidateOtp(VerifyOtpDto model);
        Task<GeneralResponse<string>> RemoveOtp(string email, string code);
    }
}
