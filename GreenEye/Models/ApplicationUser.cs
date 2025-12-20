using Microsoft.AspNetCore.Identity;

namespace GreenEye.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? ImageUrl { get; set; }
        public string? Address { get; set; }

        public List<RefreshToken>? RefreshTokens { get; set; } = new();
    }
}
