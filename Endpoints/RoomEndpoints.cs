using Microsoft.EntityFrameworkCore;
using RoomBookingService.Data;
using RoomBookingService.Data.Models;
using RoomBookingService.Middleware;
using RoomBookingService.Models.DTOs;

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
            if (!context.IsAdmin())
                return Results.Forbid();
            
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { error = new { code = "INVALID_REQUEST", message = "Room name is required" } });
            
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
            
            return Results.Ok(new { 
                room = new { room.Id, room.Name, room.Description, room.Capacity } 
            });
        })
        .RequireAuthorization()
        .WithTags("Rooms");
    }
}