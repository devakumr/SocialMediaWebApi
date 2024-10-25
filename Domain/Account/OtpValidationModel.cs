using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Auth.Account
{
    public class OtpValidationModel
    {
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string Otp { get; set; }
    }
}
