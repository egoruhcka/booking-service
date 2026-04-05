using System.ComponentModel.DataAnnotations;

namespace RoomBookingService.Data.Models;

public class Room
{
    [Key]
    public Guid Id { get; set; }
    
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public int? Capacity { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
    public Schedule? Schedule { get; set; }
}