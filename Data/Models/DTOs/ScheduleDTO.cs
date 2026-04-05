// Models/DTOs/ScheduleDto.cs
namespace RoomBookingService.Models.DTOs;

public record CreateScheduleRequest(
    Guid RoomId,
    List<int> DaysOfWeek,
    string StartTime,
    string EndTime
);

public record ScheduleResponse(
    Guid Id,
    Guid RoomId,
    List<int> DaysOfWeek,
    string StartTime,
    string EndTime,
    DateTime? CreatedAt
);