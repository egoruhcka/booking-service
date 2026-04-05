// Models/DTOs/BookingDto.cs
namespace RoomBookingService.Models.DTOs;

public record CreateBookingRequest(Guid SlotId, bool CreateConferenceLink = false);

public record BookingResponse(
    Guid Id,
    Guid SlotId,
    Guid UserId,
    string Status,  // "active" | "cancelled"
    string? ConferenceLink,
    DateTime? CreatedAt
);