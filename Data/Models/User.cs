using System.ComponentModel.DataAnnotations;

namespace RoomBookingService.Data.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }
    
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Role { get; set; } = "user";
    
    public string? PasswordHash { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
    public bool IsDummy => Id == Services.TokenService.AdminUserId || Id == Services.TokenService.RegularUserId;
}