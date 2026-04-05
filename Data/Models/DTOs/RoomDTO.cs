// Models/DTOs/RoomDto.cs
namespace RoomBookingService.Models.DTOs;

public record CreateRoomRequest(string Name, string? Description, int? Capacity);
public record RoomResponse(Guid Id, string Name, string? Description, int? Capacity, DateTime? CreatedAt);