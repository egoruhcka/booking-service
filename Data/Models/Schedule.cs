using System.ComponentModel.DataAnnotations;

namespace RoomBookingService.Data.Models;

public class Schedule
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid RoomId { get; set; }
    
    [Required]
    public List<int> DaysOfWeek { get; set; } = new();
    
    [Required, RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$")]
    public string StartTime { get; set; } = "09:00";
    
    [Required, RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$")]
    public string EndTime { get; set; } = "18:00";
    
    public DateTime? CreatedAt { get; set; }
    
    public Room? Room { get; set; }
}