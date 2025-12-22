using GreenEye.Dto.Responses;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GreenEye.Service
{
    public class AuthenticationService(
        UserManager<ApplicationUser> _userManager,
        IOtpService _otpService,
        IConfiguration configuration,
        RoleManager<IdentityRole> _roleManager,
        AppDbContext _context,
        IMemoryCache _cache
        ) 
        : IAuthenticationService
    {

        public async Task<GeneralResponse<string>> RegisterAsync(RegisterDto model)
        {
            try
            {
                // Check for email
                var getUserByEmail = await _userManager.FindByEmailAsync(model.Email!);
                if (getUserByEmail != null)
                    return new GeneralResponse<string> { IsSuccess = false, Message = "Can not create account for this email" };

                var cacheKey = "register_data";

                // Verify identity validation(before move to verify otp page)
                var user = new ApplicationUser
                {
                    Email = model.Email,
                    UserName = model.Name,
                    PhoneNumber = model.Phone
                };
                var checkUsernameValidate = await _userManager.UserValidators.First().ValidateAsync(_userManager, user);
                var checkPasswordValidate = await _userManager.PasswordValidators.First().ValidateAsync(_userManager, user, model.Password);

                // Display validations errors if found
                if(!checkUsernameValidate.Succeeded)
                {
                    foreach(var error in checkUsernameValidate.Errors)
                        return new GeneralResponse<string> { IsSuccess = false, Message = $"{error.Description}" };
                }
                if (!checkPasswordValidate.Succeeded)
                {
                    foreach (var error in checkPasswordValidate.Errors)
                        return new GeneralResponse<string> { IsSuccess = false, Message = $"{error.Description}" };
                }

                // Serialize register data
                var serialized = JsonSerializer.Serialize(model);

                // Set register data to memory cache
                _cache.Set(cacheKey,
                    serialized,
                    TimeSpan.FromMinutes(100) // data stay in memory 100 minutes
                    );

                // Generate and send OTP
                await _otpService.GenerateAndSendOtp(model.Email!, OtpType.EmailVerification);
                return new GeneralResponse<string>
                {
                    IsSuccess = true,
                    Message = "OTP send to your email, check your email."
                };
            }
            catch(Exception ex)
            {
                return new GeneralResponse<string> { IsSuccess = false, Message = $"Unhandeled exception throw.  {ex.Message}" };
                throw;
            }
        }

        public async Task<GeneralResponse<AuthResponse>> VerifyOTP(VerifyOtpDto verifyOtpDto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var otpValid = await _otpService.ValidateOtp(verifyOtpDto);
                if (!otpValid.IsSuccess)
                    return new GeneralResponse<AuthResponse> { IsSuccess = false, Message = otpValid.Message }; 


                if (verifyOtpDto.Type == OtpType.EmailVerification)
                {
                    var result = await CreateUserAsync();

                    if (!result.IsSuccess)
                        return new GeneralResponse<AuthResponse> { IsSuccess = false, Message = result.Message };

                    await _otpService.RemoveOtp(verifyOtpDto.Email!, verifyOtpDto.Code!);
                    _cache.Remove("register_data");
                    await transaction.CommitAsync();

                    return new GeneralResponse<AuthResponse> { IsSuccess = true, Data = result.Data };
                }

                else if (verifyOtpDto.Type == OtpType.ResetPassword)
                {
                    await _otpService.RemoveOtp(verifyOtpDto.Email!, verifyOtpDto.Code!);
                    await transaction.CommitAsync();
                    return new GeneralResponse<AuthResponse> { IsSuccess = true, Message = "Verfiy email for reset password successfully", Data = null};
                }
                return new GeneralResponse<AuthResponse>
                {
                    IsSuccess = false,
                    Message = "Invalid OTP type"
                };
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();

                return new GeneralResponse<AuthResponse>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
            
        }

        public async Task<GeneralResponse<AuthResponse>> CreateUserAsync()
        {
            try
            {
                var cacheKey = "register_data";

                // Get data from memory chache
                if (!_cache.TryGetValue(cacheKey, out string? cachedData))
                    return new GeneralResponse<AuthResponse> { IsSuccess = false, Message = "Data become not found rgister again" };

                // Deserialization user register data
                var data = JsonSerializer.Deserialize<RegisterDto>(cachedData!);
                // Mapping
                var user = new ApplicationUser
                {
                    Email = data!.Email,
                    UserName = data.Name,
                    PhoneNumber = data.Phone,
                    Address = data.Address,
                };

                // Create user
                var result = await _userManager.CreateAsync(user, data.Password!);

                if (result.Succeeded)
                {
                    var roleName = Enum.GetName(typeof(Roles), data.Roles);
                    if (string.IsNullOrEmpty(roleName))
                        return new GeneralResponse<AuthResponse> { IsSuccess = false, Message = "Invalid role" };

                    // Role existing check
                    var roleExists = await _roleManager.RoleExistsAsync(roleName);
                    if (!roleExists)
                        return new GeneralResponse<AuthResponse> { IsSuccess = false, Message = "Role dose not exist" };
                    // Add role for user
                    var roles = await _userManager.AddToRoleAsync(user, roleName);

                    // Prepare response
                    var authResponse = new AuthResponse();
                    var jwtToken = await GenerateToken(user);
                    var refreshToken = GenerateRefreshToken();

                    authResponse.IsAuthenticated = true;
                    authResponse.Roles = new List<string> { roleName };
                        // User Info
                    authResponse.UserName = user.UserName;
                    authResponse.Email = user.Email;
                    authResponse.UserId = user.Id;
                    authResponse.PhoneNumber = user.PhoneNumber;
                    authResponse.Address = user.Address;
                        // Token Info
                    authResponse.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                    authResponse.RefreshTokenExpiration = refreshToken.ExpiresOn;
                    authResponse.ExpiresIn = jwtToken.ValidTo.ToLocalTime();

                    authResponse.RefreshToken = refreshToken.Token;
                    user.RefreshTokens?.Add(refreshToken);
                    await _userManager.UpdateAsync(user);

                    return new GeneralResponse<AuthResponse> { IsSuccess = true, Data = authResponse };
                }

                // get Identity errors
                var errors = string.Join(" | ", result.Errors.Select(e => e.Description));

                return new GeneralResponse<AuthResponse>
                {
                    IsSuccess = false,
                    Message = errors
                };
            }
            catch(Exception ex)
            {
                return new GeneralResponse<AuthResponse> { IsSuccess = false, Message = ex.Message };
            }
            
        }

        public async Task<GeneralResponse<string>> ForgetPassword(string email)
        {
            try
            {
                if (email is not null)
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    if (user == null)
                        return new GeneralResponse<string> { IsSuccess = false, Message = "User not found" };

                    await _otpService.GenerateAndSendOtp(email, Enums.OtpType.ResetPassword);
                    return new GeneralResponse<string> { IsSuccess = true, Message = "Check for email and submit otp" };
                }
                return new GeneralResponse<string> { IsSuccess = false, Message = "User not found" };
            }
            catch(Exception ex)
            {
                return new GeneralResponse<string> { IsSuccess = false, Message = ex.Message };
            }
            
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email!);
                if (user is null)
                    return false;

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var resetPassword = await _userManager.ResetPasswordAsync(user, token, resetPasswordDto.Password!);
                return resetPassword.Succeeded ? true : false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
                throw;
            }

        }

        public async Task<GeneralResponse<AuthResponse>> Login(LoginDto loginDTO)
        {
            try
            {
                // get user and verify email
                var getUser = await _userManager.FindByEmailAsync(loginDTO.Email!);
                if (getUser is null)
                    return new GeneralResponse<AuthResponse> { IsSuccess = false, Message = "Invalid Email or Password" };

                // password verify
                var verifyPassword = await _userManager.CheckPasswordAsync(getUser, loginDTO.Password!);
                if (!verifyPassword)
                    return new GeneralResponse<AuthResponse> { IsSuccess = false, Message = "Invalid Email or Password" };

                // Prepare response
                var authResponse = new AuthResponse();
                var token = await GenerateToken(getUser);
                var roles = await _userManager.GetRolesAsync(getUser);
                    // Token Info
                authResponse.IsAuthenticated = true;
                authResponse.AccessToken = new JwtSecurityTokenHandler().WriteToken(token);
                authResponse.ExpiresIn = token.ValidTo.ToLocalTime();
                    // User Info
                authResponse.UserName = getUser.UserName;
                authResponse.Email = getUser.Email;
                authResponse.UserId = getUser.Id;
                authResponse.PhoneNumber = getUser.PhoneNumber;
                authResponse.Address = getUser.Address;
                authResponse.Roles = roles.ToList();


                if (getUser.RefreshTokens!.Any(x => x.IsActive))
                {
                    var activeRefreshToken = getUser.RefreshTokens!.FirstOrDefault(x => x.IsActive);
                    authResponse.RefreshToken = activeRefreshToken!.Token;
                    authResponse.RefreshTokenExpiration = activeRefreshToken!.ExpiresOn;
                }
                else
                {
                    var refreshToken = GenerateRefreshToken();
                    authResponse.RefreshToken = refreshToken.Token;
                    authResponse.RefreshTokenExpiration = refreshToken.ExpiresOn;

                    getUser.RefreshTokens!.Add(refreshToken);
                    await _userManager.UpdateAsync(getUser);
                }
                    

                return new GeneralResponse<AuthResponse> { IsSuccess = true, Data = authResponse };
            }
            catch(Exception ex)
            {
                return new GeneralResponse<AuthResponse>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
                throw;
            }
        }

        public async Task<GeneralResponse<string>> ResendOtpAsync(ResendOtpDto resendOtpDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(resendOtpDto.Email!);

                if (resendOtpDto.Type == OtpType.EmailVerification)
                {
                    if(user != null)
                        return new GeneralResponse<string> { IsSuccess = false, Message = "this email already have account" };

                    if (!_cache.TryGetValue("register_data", out string? cacheData))
                        return new GeneralResponse<string> { IsSuccess = false, Message = "Data become expiried. Rgister again" };

                    var data = JsonSerializer.Deserialize<ResendOtpDto>(cacheData!);
                    if(resendOtpDto.Email != data!.Email)
                        return new GeneralResponse<string> { IsSuccess = false, Message = "Incorrect match email" };

                    await _otpService.GenerateAndSendOtp(data!.Email!, data.Type);

                    return new GeneralResponse<string>
                    {
                        IsSuccess = true,
                        Message = "OTP resent successfully"
                    };
                }

                if (user == null) return new GeneralResponse<string> { IsSuccess = false, Message = "User not found" };

                if (resendOtpDto.Email == null)
                    return new GeneralResponse<string> { IsSuccess = false, Message = "Required email" };

                if (string.IsNullOrEmpty(resendOtpDto.Type.ToString()))
                    return new GeneralResponse<string> { IsSuccess = false, Message = "Type Required" };

                

                await _otpService.GenerateAndSendOtp(resendOtpDto.Email!, resendOtpDto.Type);

                return new GeneralResponse<string>
                {
                    IsSuccess = true,
                    Message = "OTP resent successfully"
                };
            }
            catch(Exception ex)
            {
                return new GeneralResponse<string> { IsSuccess = false, Message = ex.Message};
            }
        }

        public async Task<GeneralResponse<string>> RevokeTokenAsync(string token)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(x => x.RefreshTokens!.Any(x => x.Token == token));
            if (user == null)
                return new GeneralResponse<string> { IsSuccess = false, Message = "Invalid token" };

            var refreshToken = user.RefreshTokens!.Single(x => x.Token == token);
            if(!refreshToken.IsActive)
                return new GeneralResponse<string> { IsSuccess = false, Message = "InActive token" };

            // became revoked after these lines
            refreshToken.IsRevoked = true;
            refreshToken.RevokedOn = DateTime.UtcNow.ToLocalTime();

            await _userManager.UpdateAsync(user);

            return new GeneralResponse<string> { IsSuccess = true, Message = "Token became revoked" };
        }

        public async Task<GeneralResponse<AuthResponse>> RefreshTokenAsync(string token)
        {
            var authResponse = new AuthResponse();

            var user = await _context.Users.SingleOrDefaultAsync(x => x.RefreshTokens!.Any(x => x.Token == token));
            if(user is null)
                return new GeneralResponse<AuthResponse> { IsSuccess = false, Message = "Invalid token" };

            var refreshToken = user.RefreshTokens!.Single(x => x.Token == token);
            if(!refreshToken.IsActive)
                return new GeneralResponse<AuthResponse> { IsSuccess = false, Message = "InActive token" };

            refreshToken.IsRevoked = true;
            refreshToken.RevokedOn = DateTime.UtcNow;

            var roles = await _userManager.GetRolesAsync(user);

            // Prepare refresh token(generate and added to DB)
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens!.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);
            authResponse.RefreshToken = refreshToken.Token;
            authResponse.RefreshTokenExpiration = newRefreshToken.ExpiresOn;
            authResponse.IsAuthenticated = true;

            // Prepare JWT token(generate)
            var accessToken = await GenerateToken(user);
            authResponse.AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken);
            authResponse.ExpiresIn = accessToken.ValidTo.ToLocalTime();
            
            // Prepare user information
            authResponse.Address = user.Address;
            authResponse.Email = user.Email;
            authResponse.UserName = user.UserName;
            authResponse.PhoneNumber = user.PhoneNumber;
            authResponse.UserId = user.Id;
            authResponse.Roles = roles.ToList();

            return new GeneralResponse<AuthResponse> { IsSuccess = true, Message = "New refresh token created", Data = authResponse};
        }

        // Generate Token 
        private async Task<JwtSecurityToken> GenerateToken(ApplicationUser applicationUser)
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
                expires: DateTime.UtcNow.AddDays(1),
                claims: claims,
                signingCredentials: credentials
                );

            return token;
        }

        //Generate Refresh Token
        private RefreshToken GenerateRefreshToken()
        {
            var randomBytes = new byte[64];

            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomBytes);
            }
            string token = Convert.ToBase64String(randomBytes);

            return new RefreshToken
            {
                Token = token,
                ExpiresOn = DateTime.UtcNow.AddDays(3),
                CreatedOn = DateTime.UtcNow,
            };
        }
    }
}
