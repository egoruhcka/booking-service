using System.ComponentModel.DataAnnotations;

namespace RoomBookingService.Data.Models;

public enum BookingStatus { Active, Cancelled }

public class Booking
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid SlotId { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public BookingStatus Status { get; set; } = BookingStatus.Active;
    
    public string? ConferenceLink { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
    public Slot? Slot { get; set; }
    public User? User { get; set; }
}