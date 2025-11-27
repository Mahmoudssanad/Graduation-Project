using GreenEye.Enums;

namespace GreenEye.Models
{
    public class OTP
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Code { get; set; }
        public DateTime ExpireAt { get; set; }
        public bool IsUsed { get; set; }
        public OtpType Type { get; set; }
    }
}
