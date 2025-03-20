using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

public class UserModel : IdentityUser
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string LastName { get; set; }

    [Required]
    [StringLength(10)]
    public string Gender { get; set; }

   
    public int UserStatus { get; set; } = 0; // 0 = Unverified, 1 = Verified

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
