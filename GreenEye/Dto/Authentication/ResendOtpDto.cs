using GreenEye.Enums;
using System.ComponentModel.DataAnnotations;

namespace GreenEye.Dto.Authentication
{
    public class ResendOtpDto
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [Required]
        public OtpType Type { get; set; }
    }
}
