using GreenEye.Data;
using GreenEye.Dto;
using GreenEye.Dto.Authentication;
using GreenEye.Enums;
using GreenEye.Models;
using GreenEye.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace GreenEye.Service
{
    public class OtpService(AppDbContext _context, IEmailService _emailService) : IOtpService
    {
        public async Task GenerateAndSendOtp(string email, OtpType type)
        {
            var code = new Random().Next(100000, 999999).ToString();

            try
            {
                var getOtp = await _context.OTPs.FirstOrDefaultAsync(x => x.Email == email);
                if(getOtp  != null)
                {
                    getOtp.Code = code;
                    getOtp.IsUsed = false;
                    getOtp.ExpireAt = DateTime.Now.AddMinutes(5);

                    var result = _context.OTPs.Update(getOtp);
                }
                else
                {
                    var otp = new OTP
                    {
                        Email = email,
                        Code = code,
                        ExpireAt = DateTime.Now.AddMinutes(5),
                        IsUsed = false,
                        Type = type
                    };

                    var createOtp = await _context.OTPs.AddAsync(otp);
                }
                await _context.SaveChangesAsync();
                await _emailService.SendEmailAsync(email, $"{type} Otp", $"Your otp to verfiy email is {code}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<GeneralResponse<string>> ValidateOtp(VerifyOtpDto model)
        {

            var otp = await _context.OTPs.FirstOrDefaultAsync(x => x.Email == model.Email );

            if (otp == null)
                return new GeneralResponse<string>
                {
                    IsSuccess = false,
                    Message = "OTP not found"
                };

            if (otp.IsUsed)
                return new GeneralResponse<string>
                {
                    IsSuccess = false,
                    Message = "OTP already used"
                };

            if (otp.ExpireAt < DateTime.UtcNow)
                return new GeneralResponse<string>
                {
                    IsSuccess = false,
                    Message = "OTP expired"
                };

            otp.IsUsed = true;
            await _context.SaveChangesAsync();

            return new GeneralResponse<string>
            {
                IsSuccess = true,
                Message = "OTP verified successfully"
            };
        }

        

    }
}
