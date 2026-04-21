using RoomBookingService.Data.Models;

namespace RoomBookingService.Services;

public interface ITokenService
{
    string GenerateToken(User user);
    Guid? ValidateToken(string token);
    string? GetRoleFromToken(string token);
}