using Microsoft.EntityFrameworkCore;
using RoomBookingService.Data;
using RoomBookingService.Data.Models;
using RoomBookingService.Models.DTOs;
using RoomBookingService.Services;

namespace RoomBookingService.Endpoints;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/bookings/create", async (
            CreateBookingRequest request,
            AppDbContext db,
            ISlotGenerator slotGenerator,
            HttpContext context) =>
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (userId != TokenService.RegularUserId.ToString())
                return Results.Forbid();
            
            var userGuid = Guid.Parse(userId);
            
            var slot = await slotGenerator.GetSlotByIdAsync(request.SlotId);
            if (slot == null)
                return Results.NotFound();
            
            if (slot.Start < DateTime.UtcNow)
                return Results.BadRequest(new { error = new { code = "INVALID_REQUEST", message = "Cannot book a slot in the past" } });
            
            var existingBooking = await db.Bookings
                .AnyAsync(b => b.SlotId == request.SlotId && b.Status == BookingStatus.Active);
            
            if (existingBooking)
                return Results.StatusCode(409);
            
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                SlotId = request.SlotId,
                UserId = userGuid,
                Status = BookingStatus.Active,
                ConferenceLink = null,
                CreatedAt = DateTime.UtcNow
            };
            
            db.Bookings.Add(booking);
            await db.SaveChangesAsync();
            
            return Results.Ok(new { 
                booking = new { 
                    booking.Id, 
                    booking.SlotId, 
                    booking.UserId,
                    Status = booking.Status.ToString().ToLower(),
                    booking.ConferenceLink,
                    booking.CreatedAt 
                } 
            });
        })
        .RequireAuthorization()
        .WithTags("Bookings");

        app.MapGet("/bookings/my", async (
            AppDbContext db,
            ISlotGenerator slotGenerator,
            HttpContext context) =>
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (userId != TokenService.RegularUserId.ToString())
                return Results.Forbid();
            
            var userGuid = Guid.Parse(userId);
            var now = DateTime.UtcNow;
            
            var bookings = await db.Bookings
                .AsNoTracking()
                .Include(b => b.Slot)
                .Where(b => b.UserId == userGuid && 
                           b.Status == BookingStatus.Active &&
                           b.Slot!.Start >= now)
                .OrderBy(b => b.Slot!.Start)
                .Select(b => new {
                    b.Id, b.SlotId, b.UserId,
                    Status = b.Status.ToString().ToLower(),
                    b.ConferenceLink, b.CreatedAt
                })
                .ToListAsync();
            
            return Results.Ok(new { bookings });
        })
        .RequireAuthorization()
        .WithTags("Bookings");

        app.MapGet("/bookings/list", async (
            int page,
            int pageSize,
            AppDbContext db,
            HttpContext context) =>
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (userId != TokenService.AdminUserId.ToString())
                return Results.Forbid();
            
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);
            
            var query = db.Bookings.AsNoTracking().Include(b => b.Slot);
            
            var total = await query.CountAsync();
            var bookings = await query
                .OrderBy(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new {
                    b.Id, b.SlotId, b.UserId,
                    Status = b.Status.ToString().ToLower(),
                    b.ConferenceLink, b.CreatedAt
                })
                .ToListAsync();
            
            return Results.Ok(new { 
                bookings,
                pagination = new { page, pageSize, total }
            });
        })
        .RequireAuthorization()
        .WithTags("Bookings");

        app.MapPost("/bookings/{bookingId:guid}/cancel", async (
            Guid bookingId,
            AppDbContext db,
            HttpContext context) =>
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (userId != TokenService.RegularUserId.ToString())
                return Results.Forbid();
            
            var userGuid = Guid.Parse(userId);
            
            var booking = await db.Bookings.FindAsync(bookingId);
            if (booking == null)
                return Results.NotFound();
            
            if (booking.UserId != userGuid)
                return Results.Forbid();
            
            if (booking.Status == BookingStatus.Cancelled)
            {
                return Results.Ok(new { 
                    booking = new { 
                        booking.Id, booking.SlotId, booking.UserId,
                        Status = booking.Status.ToString().ToLower(),
                        booking.ConferenceLink, booking.CreatedAt 
                    } 
                });
            }
            
            booking.Status = BookingStatus.Cancelled;
            await db.SaveChangesAsync();
            
            return Results.Ok(new { 
                booking = new { 
                    booking.Id, booking.SlotId, booking.UserId,
                    Status = booking.Status.ToString().ToLower(),
                    booking.ConferenceLink, booking.CreatedAt 
                } 
            });
        })
        .RequireAuthorization()
        .WithTags("Bookings");
    }
}