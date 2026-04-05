using Microsoft.EntityFrameworkCore;
using RoomBookingService.Data;
using RoomBookingService.Data.Models;
using RoomBookingService.Services;

namespace RoomBookingService.Endpoints;

public static class SlotEndpoints
{
    public static void MapSlotEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/rooms/{roomId:guid}/slots/list", async (
            Guid roomId,
            string date,
            AppDbContext db,
            ISlotGenerator slotGenerator,
            HttpContext context) =>
        {
            if (!DateOnly.TryParse(date, out var parsedDate))
                return Results.BadRequest(new { error = new { code = "INVALID_REQUEST", message = "Invalid date format. Use YYYY-MM-DD" } });
            
            var room = await db.Rooms.FindAsync(roomId);
            if (room == null)
                return Results.NotFound();
            
            var slots = await slotGenerator.GetOrCreateSlotsForDateAsync(roomId, parsedDate);
            
            var slotIds = slots.Select(s => s.Id).ToList();
            
            var bookedSlotIds = await db.Bookings
                .AsNoTracking()
                .Where(b => b.Status == BookingStatus.Active && 
                           slotIds.Contains(b.SlotId))
                .Select(b => b.SlotId)
                .ToListAsync();
            
            var availableSlots = slots
                .Where(s => !bookedSlotIds.Contains(s.Id))
                .Select(s => new { 
                    s.Id, 
                    s.RoomId, 
                    Start = s.Start.ToString("o"),
                    End = s.End.ToString("o") 
                });
            
            return Results.Ok(new { slots = availableSlots });
        })
        .RequireAuthorization()
        .WithTags("Slots");
    }
}