using Microsoft.AspNetCore.Http.HttpResults;
using RoomBookingService.Data;
using RoomBookingService.Data.Models;
using RoomBookingService.Services;

namespace RoomBookingService.Endpoints;

public record DummyLoginRequest(string Role);
public record TokenResponse(string Token);

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/dummyLogin", async Task<Results<Ok<TokenResponse>, BadRequest<string>>> (
            DummyLoginRequest request,
            ITokenService tokenService,
            AppDbContext db) =>
        {
            if (request.Role is not "admin" and not "user")
                return TypedResults.BadRequest("Role must be 'admin' or 'user'");
            
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
            return TypedResults.Ok(new TokenResponse(token));
        })
        .AllowAnonymous()
        .WithTags("Auth");
    }
}