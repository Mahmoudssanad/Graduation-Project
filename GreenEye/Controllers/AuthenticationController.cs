using GreenEye.Dto;
using GreenEye.Dto.Authentication;
using GreenEye.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace GreenEye.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IAuthenticationService _authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<GeneralResponse<string>>> Register(RegisterDto model)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.RegisterAsync(model);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            return BadRequest();
        }

        [HttpPost("verify-otp")]
        public async Task<ActionResult<GeneralResponse<string>>> VerifyOTP(VerifyOtpDto verifyOtpDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.VerifyOTP(verifyOtpDto);
                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            return BadRequest(new GeneralResponse<string> { IsSuccess = false, Message = "Invalid data"});
        }

        [HttpPost("login")]
        public async Task<ActionResult<GeneralResponse<string>>> Login(LoginDto loginDto)
        {
            if(ModelState.IsValid)
            {
                var result = await _authService.Login(loginDto);
                
                return result.IsSuccess ? Ok(result)  : BadRequest(result);
            }
            return BadRequest();
        }

        [HttpPost("forget-password")]
        public async Task<ActionResult<GeneralResponse<string>>> ForgetPassword(string email)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.ForgetPassword(email);
                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            return BadRequest();
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<GeneralResponse<string>>> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.ResetPasswordAsync(resetPasswordDto);

                return result ? Ok(new GeneralResponse<string> { IsSuccess = true, Message = "Reset password successfully" })
                : BadRequest(new GeneralResponse<string> { IsSuccess = false, Message = "Error occured when reset password" });
            }
            return BadRequest();
        }

        [HttpPost("resend-otp")]
        public async Task<ActionResult<GeneralResponse<string>>> ResendOtp(ResendOtpDto resendOtpDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new GeneralResponse<string> { IsSuccess = false, Message = "Invalid data"});

            var result = await _authService.ResendOtpAsync(resendOtpDto);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
