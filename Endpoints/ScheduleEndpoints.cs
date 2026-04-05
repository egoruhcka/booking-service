using Microsoft.EntityFrameworkCore;
using RoomBookingService.Data;
using RoomBookingService.Data.Models;
using RoomBookingService.Middleware;
using RoomBookingService.Models.DTOs;
using System.Text.RegularExpressions;

namespace RoomBookingService.Endpoints;

public static class ScheduleEndpoints
{
    private static bool IsValidTime(string time) => 
        Regex.IsMatch(time, @"^([01]?[0-9]|2[0-3]):[0-5][0-9]$");
    
    public static void MapScheduleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/rooms/{roomId:guid}/schedule/create", async (
            Guid roomId,
            CreateScheduleRequest request,
            AppDbContext db,
            HttpContext context) =>
        {
            if (!context.IsAdmin())
                return Results.Forbid();
            
            var room = await db.Rooms.FindAsync(roomId);
            if (room == null)
                return Results.NotFound();
            
            var existing = await db.Schedules.AnyAsync(s => s.RoomId == roomId);
            if (existing)
                return Results.StatusCode(409);
            
            if (request.DaysOfWeek == null || request.DaysOfWeek.Count == 0 || 
                request.DaysOfWeek.Any(d => d < 1 || d > 7))
                return Results.BadRequest(new { error = new { code = "INVALID_REQUEST", message = "daysOfWeek must contain values 1-7" } });
            
            if (!IsValidTime(request.StartTime) || !IsValidTime(request.EndTime))
                return Results.BadRequest(new { error = new { code = "INVALID_REQUEST", message = "Time must be in HH:MM format" } });
            
            if (request.EndTime.CompareTo(request.StartTime) <= 0)
                return Results.BadRequest(new { error = new { code = "INVALID_REQUEST", message = "endTime must be after startTime" } });
            
            var schedule = new Schedule
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                DaysOfWeek = request.DaysOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                CreatedAt = DateTime.UtcNow
            };
            
            db.Schedules.Add(schedule);
            await db.SaveChangesAsync();
            
            return Results.Ok(new { 
                schedule = new { 
                    schedule.Id, schedule.RoomId, schedule.DaysOfWeek, 
                    schedule.StartTime, schedule.EndTime, schedule.CreatedAt 
                } 
            });
        })
        .RequireAuthorization()
        .WithTags("Schedules");
    }
}