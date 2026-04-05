using BCrypt.Net;

namespace RoomBookingService.Services;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;
    
    public string HashPassword(string password) => 
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    
    public bool VerifyPassword(string password, string passwordHash) => 
        BCrypt.Net.BCrypt.Verify(password, passwordHash);
}