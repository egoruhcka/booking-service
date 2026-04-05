using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RoomBookingService.Data.Models;

namespace RoomBookingService.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _key;
    public static readonly Guid AdminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid RegularUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public TokenService(IConfiguration config)
    {
        _config = config;
        var jwtKey = config["Jwt:Key"] ?? "super-secret-key-for-test-task-min-32-chars!!";
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    }
    
    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("user_id", user.Id.ToString()),
            new Claim("role", user.Role),
            new Claim(ClaimTypes.Role, user.Role)
        };
        
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public Guid? ValidateToken(string token) => null;
    public string? GetRoleFromToken(string token) => null;
}