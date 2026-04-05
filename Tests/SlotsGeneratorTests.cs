using Xunit;
using System.Security.Cryptography;
using System.Text;

namespace RoomBookingService.Tests.Unit;

public class SlotGeneratorTests
{
    [Fact]
    public void GenerateStableSlotId_SameInput_SameOutput()
    {
        var roomId = Guid.Parse("7b920726-2567-406b-8878-14912d666557");
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var time = TimeOnly.FromDateTime(DateTime.UtcNow.Date.AddHours(9));

        var id1 = GenerateStableSlotId(roomId, date, time);
        var id2 = GenerateStableSlotId(roomId, date, time);

        Assert.Equal(id1, id2);
    }

    [Fact]
    public void GenerateStableSlotId_DifferentRoom_DifferentOutput()
    {
        var roomId1 = Guid.Parse("7b920726-2567-406b-8878-14912d666557");
        var roomId2 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var time = TimeOnly.FromDateTime(DateTime.UtcNow.Date.AddHours(9));

        var id1 = GenerateStableSlotId(roomId1, date, time);
        var id2 = GenerateStableSlotId(roomId2, date, time);

        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void GenerateStableSlotId_DifferentDate_DifferentOutput()
    {
        var roomId = Guid.Parse("7b920726-2567-406b-8878-14912d666557");
        var date1 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var date2 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
        var time = TimeOnly.FromDateTime(DateTime.UtcNow.Date.AddHours(9));

        var id1 = GenerateStableSlotId(roomId, date1, time);
        var id2 = GenerateStableSlotId(roomId, date2, time);

        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void GenerateStableSlotId_DifferentTime_DifferentOutput()
    {
        var roomId = Guid.Parse("7b920726-2567-406b-8878-14912d666557");
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var time1 = TimeOnly.FromDateTime(DateTime.UtcNow.Date.AddHours(9));
        var time2 = TimeOnly.FromDateTime(DateTime.UtcNow.Date.AddHours(10));

        var id1 = GenerateStableSlotId(roomId, date, time1);
        var id2 = GenerateStableSlotId(roomId, date, time2);

        Assert.NotEqual(id1, id2);
    }

    private static Guid GenerateStableSlotId(Guid roomId, DateOnly date, TimeOnly startTime)
    {
        var input = $"{roomId}:{date:yyyy-MM-dd}:{startTime:HH:mm}";
        using var md5 = MD5.Create();
        var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(bytes.Take(16).ToArray());
    }
}