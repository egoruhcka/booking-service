using Xunit;
using RoomBookingService.Data.Models;

namespace RoomBookingService.Tests.Unit;

public class ModelTests
{
    [Fact]
    public void User_Can_Be_Instantiated()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            Role = "user",
            CreatedAt = DateTime.UtcNow
        };

        Assert.Equal("test@test.com", user.Email);
        Assert.Equal("user", user.Role);
        Assert.True(user.Id != Guid.Empty);
    }

    [Fact]
    public void Room_Can_Be_Instantiated()
    {
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Test Room",
            Description = "For testing",
            Capacity = 10,
            CreatedAt = DateTime.UtcNow
        };

        Assert.Equal("Test Room", room.Name);
        Assert.Equal(10, room.Capacity);
        Assert.True(room.Id != Guid.Empty);
    }

    [Fact]
    public void Schedule_Can_Be_Instantiated()
    {
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            RoomId = Guid.NewGuid(),
            DaysOfWeek = new List<int> { 1, 2, 3, 4, 5 },
            StartTime = "09:00",
            EndTime = "18:00",
            CreatedAt = DateTime.UtcNow
        };

        Assert.Contains(1, schedule.DaysOfWeek);
        Assert.Equal("09:00", schedule.StartTime);
        Assert.True(schedule.Id != Guid.Empty);
        Assert.True(schedule.RoomId != Guid.Empty);
    }

    [Fact]
    public void Booking_Can_Be_Instantiated()
    {
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            SlotId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = BookingStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        Assert.Equal(BookingStatus.Active, booking.Status);
        Assert.True(booking.Id != Guid.Empty);
        Assert.True(booking.SlotId != Guid.Empty);
        Assert.True(booking.UserId != Guid.Empty);
    }

    [Fact]
    public void BookingStatus_Enum_Has_Expected_Values()
    {
        Assert.Equal(0, (int)BookingStatus.Active);
        Assert.Equal(1, (int)BookingStatus.Cancelled);
    }
}