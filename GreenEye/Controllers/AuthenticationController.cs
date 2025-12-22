using GreenEye.Dto.Responses;

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
            return BadRequest(ModelState);
        }

        [HttpPost("verify-otp")]
        public async Task<ActionResult<GeneralResponse<AuthResponse>>> VerifyOTP(VerifyOtpDto verifyOtpDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.VerifyOTP(verifyOtpDto);
                if (!result.IsSuccess)
                    return BadRequest(result);

                if(verifyOtpDto.Type == OtpType.EmailVerification)
                    SetRefreshTokenInCookie(result.Data!.RefreshToken!, result.Data.RefreshTokenExpiration);

                return Ok(result);
            }
            return BadRequest(new GeneralResponse<AuthResponse> { IsSuccess = false, Message = "Invalid data"});
        }

        [HttpPost("login")]
        public async Task<ActionResult<GeneralResponse<AuthResponse>>> Login(LoginDto loginDto)
        {
            if(ModelState.IsValid)
            {
                var result = await _authService.Login(loginDto);
                if (!result.IsSuccess)
                    return BadRequest(result);

                if (!string.IsNullOrEmpty(result.Data!.RefreshToken))
                    SetRefreshTokenInCookie(result.Data.RefreshToken, result.Data.RefreshTokenExpiration);

                return Ok(result);
            }
            return BadRequest(ModelState);
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

        [HttpPost("revoke-token")]
        public async Task<ActionResult<GeneralResponse<string>>> RevokeToken([FromBody] RevokeToken revokeTokenModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = revokeTokenModel.Token ?? Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required!");

            var result = await _authService.RevokeTokenAsync(token!);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("refresh-token")]
        public async Task<ActionResult<GeneralResponse<AuthResponse>>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var result = await _authService.RefreshTokenAsync(refreshToken!);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            // Set new refresh token in cookie
            SetRefreshTokenInCookie(result.Data!.RefreshToken!, result.Data.RefreshTokenExpiration);

            return Ok(result);
        }

        private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires.ToLocalTime()
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
