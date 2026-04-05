using Xunit;
using RoomBookingService.Services;
using RoomBookingService.Data.Models;

namespace RoomBookingService.Tests.Unit;

public class TokenServiceTests
{
    private IConfiguration CreateTestConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super-secret-key-for-test-task-min-32-chars!!",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience"
            })
            .Build();

    [Fact]
    public void AdminUserId_Is_Fixed_Guid()
    {
        Assert.Equal("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", 
            TokenService.AdminUserId.ToString());
    }

    [Fact]
    public void RegularUserId_Is_Fixed_Guid()
    {
        Assert.Equal("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", 
            TokenService.RegularUserId.ToString());
    }

    [Fact]
    public void GenerateToken_Produces_NonEmpty_String()
    {
        var config = CreateTestConfig();
        var service = new TokenService(config);
        var user = new User 
        { 
            Id = Guid.NewGuid(), 
            Email = "test@test.com", 
            Role = "user" 
        };

        var token = service.GenerateToken(user);

        Assert.NotNull(token);
        Assert.True(token.Length > 100);
        Assert.Contains(".", token);
    }

    [Fact]
    public void GenerateToken_Admin_Contains_Role_Claim()
    {
        var config = CreateTestConfig();
        var service = new TokenService(config);
        var admin = new User 
        { 
            Id = TokenService.AdminUserId, 
            Email = "admin@test.com", 
            Role = "admin" 
        };

        var token = service.GenerateToken(admin);

        Assert.NotNull(token);
        var parts = token.Split('.');
        Assert.Equal(3, parts.Length);
        
        var payload = parts[1];
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        Assert.Contains("admin", decoded);
    }
}