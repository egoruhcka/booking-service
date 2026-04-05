using Microsoft.EntityFrameworkCore;
using RoomBookingService.Data;
using RoomBookingService.Data.Models;
using RoomBookingService.Models.DTOs;
using RoomBookingService.Services;

namespace RoomBookingService.Endpoints;

public static class RoomEndpoints
{
    public static void MapRoomEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/rooms/list", async (AppDbContext db) =>
        {
            var rooms = await db.Rooms
                .AsNoTracking()
                .Select(r => new RoomResponse(r.Id, r.Name, r.Description, r.Capacity, r.CreatedAt))
                .ToListAsync();
            
            return Results.Ok(new { rooms });
        })
        .RequireAuthorization()
        .WithTags("Rooms");

        app.MapPost("/rooms/create", async (
            CreateRoomRequest request,
            AppDbContext db,
            HttpContext context) =>
        {
            var userIdClaim = context.User.FindFirst("user_id")?.Value;
            var subClaim = context.User.FindFirst("sub")?.Value;
            
            Console.WriteLine($"[AUTH] user_id='{userIdClaim}', sub='{subClaim}'");
            
            var adminUuid = TokenService.AdminUserId.ToString();
            var isAdmin = userIdClaim == adminUuid || subClaim == adminUuid;
            
            Console.WriteLine($"[AUTH] IsAdmin={isAdmin}");
            
            if (!isAdmin)
                return Results.Forbid();
            
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest("Room name is required");
            
            var room = new Room
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                Capacity = request.Capacity,
                CreatedAt = DateTime.UtcNow
            };
            
            db.Rooms.Add(room);
            await db.SaveChangesAsync();
            
            Console.WriteLine($"[DEBUG] Room created: {room.Id}");
            
            return Results.Json(new { 
                room = new { room.Id, room.Name, room.Description, room.Capacity } 
            });
        })
        .RequireAuthorization()
        .WithTags("Rooms");
    }
}