using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GreenEye.Service
{
    public class AuthenticationService(
        UserManager<ApplicationUser> _userManager,
        IOtpService _otpService,
        IHttpContextAccessor _httpContext,
        IConfiguration configuration,
        RoleManager<IdentityRole> _roleManager,
        AppDbContext _context
        ) 
        : IAuthenticationService
    {

        public async Task<GeneralResponse<string>> RegisterAsync(RegisterDto model)
        {
            var getUserByEmail = await _userManager.FindByEmailAsync(model.Email!);
            if (getUserByEmail != null)
                return new GeneralResponse<string> { IsSuccess = false, Message = "Can not create account for this email"};

            // Save register data in session(Serialize)
            _httpContext.HttpContext?.Session.SetString("RegisterData", JsonConvert.SerializeObject(model));

            // Generate and send OTP
            await _otpService.GenerateAndSendOtp(model.Email!, OtpType.EmailVerification);
            return new GeneralResponse<string>
            {
                IsSuccess = true, Message = "OTP send to your email, check your email."
            };
        }

        public async Task<GeneralResponse<string>> VerifyOTP(VerifyOtpDto verifyOtpDto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var otpValid = await _otpService.ValidateOtp(verifyOtpDto);
                if (!otpValid.IsSuccess)
                    return otpValid;


                if (verifyOtpDto.Type == OtpType.EmailVerification)
                {
                    var result = await CreateUserAsync();

                    if (!result.IsSuccess)
                        return result;

                    await transaction.CommitAsync();
                    return result;
                }
                else if (verifyOtpDto.Type == OtpType.ResetPassword)
                {
                    await transaction.CommitAsync();
                    return new GeneralResponse<string> { IsSuccess = true, Message = "Verfiy email for reset password successfully" };
                }
                return new GeneralResponse<string>
                {
                    IsSuccess = false,
                    Message = "Invalid OTP type"
                };
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();

                return new GeneralResponse<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
            
        }

        public async Task<GeneralResponse<string>> CreateUserAsync()
        {
            var registerDataJson = _httpContext.HttpContext?.Session.GetString("RegisterData");

            if (registerDataJson == null)
                return new GeneralResponse<string>
                {
                    IsSuccess = false,
                    Message = "Error getting data from session"
                };

            var registerData = JsonConvert.DeserializeObject<RegisterDto>(registerDataJson);

            var user = new ApplicationUser
            {
                Email = registerData!.Email,
                UserName = registerData.Name,
                PhoneNumber = registerData.Phone,
            };

            var result = await _userManager.CreateAsync(user, registerData.Password!);


            if (result.Succeeded)
            {
                var roleName = Enum.GetName(typeof(Roles), registerData.Roles);

                if (string.IsNullOrEmpty(roleName))
                    return new GeneralResponse<string>
                    {
                        IsSuccess = false,
                        Message = "Invalid role"
                    };

                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                    return new GeneralResponse<string> { IsSuccess = false, Message = "Role dose not exist" };

                // Add role for user
                await _userManager.AddToRoleAsync(user, roleName);
                return new GeneralResponse<string>
                {
                    IsSuccess = true,
                    Message = "Create account successfully"
                };
            }

            // get Identity errors
            var errors = string.Join(" | ", result.Errors.Select(e => e.Description));

            return new GeneralResponse<string>
            {
                IsSuccess = false,
                Message = errors
            };
        }

        public async Task<GeneralResponse<string>> ForgetPassword(string email)
        {
            if(email is not null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return new GeneralResponse<string> { IsSuccess = false, Message = "User not found" };

                await _otpService.GenerateAndSendOtp(email, Enums.OtpType.ResetPassword);
                return new GeneralResponse<string> { IsSuccess = true, Message = "Check for email and submit otp" };
            }
            return new GeneralResponse<string> { IsSuccess = false, Message = "User not found" };
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email!);
            if (user is null)
                return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetPassword = await _userManager.ResetPasswordAsync(user, token, resetPasswordDto.Password!);
            return resetPassword.Succeeded ? true : false;

        }

        public async Task<GeneralResponse<string>> Login(LoginDto loginDTO)
        {
            try
            {
                // get user and verify email
                var getUser = await _userManager.FindByEmailAsync(loginDTO.Email!);
                if (getUser is null)
                    return new GeneralResponse<string> { IsSuccess = false, Message = "Invalid Email or Password" };

                // password verify
                var verifyPassword = await _userManager.CheckPasswordAsync(getUser, loginDTO.Password!);
                if (!verifyPassword)
                    return new GeneralResponse<string> { IsSuccess = false, Message = "Invalid Email or Password" };

                // generate token
                string token = await GenerateToken(getUser);

                return new GeneralResponse<string> { IsSuccess = true, Message = token };
            }
            catch(Exception ex)
            {
                return new GeneralResponse<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        // Generate Token 
        private async Task<string> GenerateToken(ApplicationUser applicationUser)
        {
            var claims = new List<Claim>()
            {
                new(ClaimTypes.NameIdentifier, applicationUser.Id),
                new(ClaimTypes.Name, applicationUser.UserName!),
                new(ClaimTypes.Email, applicationUser.Email!),
            };
            // get role
            var userRoles = await _userManager.GetRolesAsync(applicationUser);
            foreach (var role in userRoles)
            {
                claims.Add(new(ClaimTypes.Role, role));
            }

            var key = Encoding.UTF8.GetBytes(configuration["JWTAuthentication:Key"]!);

            var securitKey = new SymmetricSecurityKey(key);

            var credentials = new SigningCredentials(securitKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
                (
                issuer: configuration["JWTAuthentication:Issuer"],
                audience: configuration["JWTAuthentication:Audience"],
                expires: DateTime.Now.AddHours(3),
                claims: claims,
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<GeneralResponse<string>> ResendOtpAsync(ResendOtpDto resendOtpDto)
        {
            var user = await _userManager.FindByEmailAsync(resendOtpDto.Email!);
            if (user == null) return new GeneralResponse<string> { IsSuccess = false, Message = "User not found" };

            if (resendOtpDto.Email == null)
                return new GeneralResponse<string> { IsSuccess = false, Message = "Required email" };

            if(string.IsNullOrEmpty(resendOtpDto.Type.ToString()))
                return new GeneralResponse<string> { IsSuccess = false, Message = "Type Required" };

            await _otpService.GenerateAndSendOtp(resendOtpDto.Email!, resendOtpDto.Type);

            return new GeneralResponse<string>
            {
                IsSuccess = true,
                Message = "OTP resent successfully"
            };
        }
    }
}
