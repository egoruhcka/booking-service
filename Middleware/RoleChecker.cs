namespace RoomBookingService.Middleware;

public static class RoleChecker
{
    public static bool IsAdmin(this HttpContext context) =>
        context.User.FindFirst("role")?.Value == "admin";
    
    public static bool IsUser(this HttpContext context) =>
        context.User.FindFirst("role")?.Value == "user";
    
    public static Guid GetUserId(this HttpContext context) =>
        Guid.Parse(context.User.FindFirst("user_id")?.Value 
            ?? throw new UnauthorizedAccessException("user_id not found"));
}