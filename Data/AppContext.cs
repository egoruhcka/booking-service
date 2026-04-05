using Microsoft.EntityFrameworkCore;
using RoomBookingService.Data.Models;

namespace RoomBookingService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Slot> Slots => Set<Slot>();
    public DbSet<Booking> Bookings => Set<Booking>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Schedule>()
            .HasIndex(s => s.RoomId)
            .IsUnique();
        
        modelBuilder.Entity<Slot>()
            .HasIndex(s => new { s.RoomId, s.Start });
        
        modelBuilder.Entity<Booking>()
            .HasIndex(b => new { b.SlotId, b.Status })
            .HasFilter("\"Status\" = '0'");
        
        modelBuilder.Entity<Booking>()
            .HasIndex(b => new { b.UserId, b.Status });
    }
}