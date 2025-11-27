using GreenEye.Enums;
using System.ComponentModel.DataAnnotations;

namespace GreenEye.Dto.Authentication
{
    public class VerifyOtpDto
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [Required]
        public string? Code { get; set; }

        public OtpType Type { get; set; }
    }
}
