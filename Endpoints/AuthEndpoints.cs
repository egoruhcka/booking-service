using Microsoft.EntityFrameworkCore;
using RoomBookingService.Data;
using RoomBookingService.Data.Models;
using RoomBookingService.Services;

namespace RoomBookingService.Endpoints;

public record DummyLoginRequest(string Role);
public record RegisterRequest(string Email, string Password, string Role = "user");
public record LoginRequest(string Email, string Password);

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/dummyLogin", async (
            DummyLoginRequest request,
            AppDbContext db,
            ITokenService tokenService) =>
        {
            if (request.Role is not "admin" and not "user")
                return Results.BadRequest(new { error = new { code = "INVALID_REQUEST", message = "Role must be 'admin' or 'user'" } });
            
            var userId = request.Role == "admin" 
                ? TokenService.AdminUserId 
                : TokenService.RegularUserId;
            
            var user = await db.Users.FindAsync(userId);
            if (user == null)
            {
                user = new User
                {
                    Id = userId,
                    Email = request.Role == "admin" ? "admin@avito.test" : "user@avito.test",
                    Role = request.Role,
                    CreatedAt = DateTime.UtcNow
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }
            
            var token = tokenService.GenerateToken(user);
            return Results.Ok(new { token });
        })
        .AllowAnonymous()
        .WithTags("Auth");

        app.MapPost("/register", async (
            RegisterRequest request,
            AppDbContext db,
            IPasswordHasher passwordHasher,
            ITokenService tokenService) =>
        {
            if (request.Role != "user")
                return Results.BadRequest(new { error = new { code = "INVALID_REQUEST", message = "Only 'user' role can be self-registered" } });
            
            if (await db.Users.AnyAsync(u => u.Email == request.Email))
                return Results.Conflict(new { error = new { code = "USER_EXISTS", message = "User with this email already exists" } });
            
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
                return Results.BadRequest(new { error = new { code = "INVALID_REQUEST", message = "Password must be at least 8 characters" } });
            
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email.ToLowerInvariant(),
                Role = request.Role,
                PasswordHash = passwordHasher.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };
            
            db.Users.Add(user);
            await db.SaveChangesAsync();
            
            var token = tokenService.GenerateToken(user);
            
            return Results.Ok(new { token, role = user.Role });
        })
        .AllowAnonymous()
        .WithTags("Auth");

        app.MapPost("/login", async (
            LoginRequest request,
            AppDbContext db,
            IPasswordHasher passwordHasher,
            ITokenService tokenService) =>
        {
            var user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());
            
            if (user == null)
                return Results.Unauthorized();
            
            if (user.IsDummy)
                return Results.BadRequest(new { error = new { code = "INVALID_REQUEST", message = "Use /dummyLogin for test accounts" } });
            
            if (string.IsNullOrEmpty(user.PasswordHash) || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
                return Results.Unauthorized();
            
            var token = tokenService.GenerateToken(user);
            
            return Results.Ok(new { token, role = user.Role });
        })
        .AllowAnonymous()
        .WithTags("Auth");
    }
}