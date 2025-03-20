using System.ComponentModel.DataAnnotations;

namespace Domain.DtoClass
{
    public class OtpValidationDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "OTP is required.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits.")]
        public required string Otp { get; set; }
    }
}
