using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class UserOTPModel
{
    [Key]
    public int Id { get; set; }  // Primary Key

    [Required]
    [ForeignKey("User")]  // Foreign Key
    public string UserId { get; set; }

    public string Otp { get; set; }

    public DateTime ExpirationTime { get; set; }

    public bool IsUsed { get; set; } = false; // ✅ Track whether OTP is used

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public virtual UserModel User { get; set; }
}
