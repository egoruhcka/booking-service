namespace RoomBookingService.Services;
using RoomBookingService.Data.Models;


public interface ITokenService
{
    string GenerateToken(User user);
    Guid? ValidateToken(string token);
    string? GetRoleFromToken(string token);
}