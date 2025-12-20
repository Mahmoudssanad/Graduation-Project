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
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Phone number must be 11 number")]
        public string? Phone { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Address { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required]
        [Compare("Password")]
        public string? ConfirmPassword { get; set; }

        [Required]
        public Roles Roles {  get; set; }
    }
}
