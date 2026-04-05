using System.ComponentModel.DataAnnotations;

namespace RoomBookingService.Data.Models;

public class Slot
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid RoomId { get; set; }
    
    [Required]
    public DateTime Start { get; set; }
    
    [Required]
    public DateTime End { get; set; }
    
    public Room? Room { get; set; }
    
    public ICollection<Booking>? Bookings { get; set; }
}