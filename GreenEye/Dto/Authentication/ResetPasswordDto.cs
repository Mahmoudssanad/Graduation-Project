using System.ComponentModel.DataAnnotations;

namespace GreenEye.Dto.Authentication
{
    public class ResetPasswordDto
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
