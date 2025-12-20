namespace GreenEye.Dto.Responses
{
    public class AuthResponse
    {
        public bool IsAuthenticated { get; set; }

        // UserInfo
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? UserId { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string>? Roles { get; set; }

        // Token Info
        public string? AccessToken { get; set; }
        public DateTime ExpiresIn { get; set; }

        [JsonIgnore]
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
