using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace RoomBookingService.Middleware;

public static class RoleChecker
{
    public static bool IsAdmin(this HttpContext context)
    {
        var role = context.User.FindFirst("role")?.Value 
                ?? context.User.FindFirst(ClaimTypes.Role)?.Value;
        return role == "admin";
    }
    
    public static bool IsUser(this HttpContext context)
    {
        var role = context.User.FindFirst("role")?.Value 
                ?? context.User.FindFirst(ClaimTypes.Role)?.Value;
        return role == "user";
    }
    
    public static Guid GetUserId(this HttpContext context) =>
        Guid.Parse(
            context.User.FindFirst("user_id")?.Value 
            ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("user_id not found")
        );
}