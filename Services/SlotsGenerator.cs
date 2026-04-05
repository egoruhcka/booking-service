using RoomBookingService.Data;
using RoomBookingService.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace RoomBookingService.Services;

public interface ISlotGenerator
{
    Task<List<Slot>> GetOrCreateSlotsForDateAsync(Guid roomId, DateOnly date);
    Task<Slot?> GetSlotByIdAsync(Guid slotId);
}

public class SlotGenerator : ISlotGenerator
{
    private readonly AppDbContext _db;
    
    public SlotGenerator(AppDbContext db) => _db = db;
    
    public async Task<List<Slot>> GetOrCreateSlotsForDateAsync(Guid roomId, DateOnly date)
    {
        var existingSlots = await _db.Slots
            .AsNoTracking()
            .Where(s => s.RoomId == roomId && 
                       DateOnly.FromDateTime(s.Start) == date)
            .ToListAsync();
        
        if (existingSlots.Any())
            return existingSlots;
        
        var schedule = await _db.Schedules
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.RoomId == roomId);
        
        if (schedule == null)
            return new List<Slot>();
        
        var dayOfWeek = (int)date.DayOfWeek;
        var dayOfWeekTz = dayOfWeek == 0 ? 7 : dayOfWeek;
        
        if (!schedule.DaysOfWeek.Contains(dayOfWeekTz))
            return new List<Slot>();
        
        var slots = GenerateSlots(schedule, date);
        
        foreach (var slot in slots)
        {
            if (!await _db.Slots.AnyAsync(s => s.Id == slot.Id))
            {
                _db.Slots.Add(slot);
            }
        }
        
        await _db.SaveChangesAsync();
        
        return slots;
    }
    
    public async Task<Slot?> GetSlotByIdAsync(Guid slotId) =>
        await _db.Slots.FindAsync(slotId);
    private static List<Slot> GenerateSlots(Schedule schedule, DateOnly date)
    {
        var slots = new List<Slot>();
        var startTime = TimeOnly.Parse(schedule.StartTime);
        var endTime = TimeOnly.Parse(schedule.EndTime);
        
        var current = startTime;
        while (current < endTime)
        {
            var slotEnd = current.AddMinutes(30);
            if (slotEnd > endTime) break;
            
            var slotId = GenerateStableSlotId(schedule.RoomId, date, current);
            
            var startUtc = DateTime.SpecifyKind(
                new DateTime(date.Year, date.Month, date.Day, current.Hour, current.Minute, 0), 
                DateTimeKind.Utc);
            
            var endUtc = DateTime.SpecifyKind(
                new DateTime(date.Year, date.Month, date.Day, slotEnd.Hour, slotEnd.Minute, 0), 
                DateTimeKind.Utc);
            
            slots.Add(new Slot
            {
                Id = slotId,
                RoomId = schedule.RoomId,
                Start = startUtc,
                End = endUtc
            });
            
            current = slotEnd;
        }
        
        return slots;
    }
    private static Guid GenerateStableSlotId(Guid roomId, DateOnly date, TimeOnly startTime)
    {
        var input = $"{roomId}:{date:yyyy-MM-dd}:{startTime:HH:mm}";
        using var md5 = MD5.Create();
        var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(bytes.Take(16).ToArray());
    }
}   