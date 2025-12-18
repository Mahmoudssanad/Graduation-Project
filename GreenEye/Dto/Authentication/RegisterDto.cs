using GreenEye.Enums;
using System.ComponentModel.DataAnnotations;

namespace GreenEye.Dto.Authentication
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(255)]
        [MinLength(3)]
        public string? Name { get; set; }

        [Required]
        [MaxLength(11)]
        [MinLength(11)]
        public string? Phone { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required]
        public Roles Roles {  get; set; }
    }
}
