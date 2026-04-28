using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

public static class RegisterEndpoints
{
    public static void MapRegisterEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/register", async (RegisterRequest request, DatabaseContext context) =>
        {
            if (await context.Users.CountAsync(x => x.UsernameHash == request.UsernameHash) > 0)
                return Results.Conflict();

            context.Users.Add(new User
            {
                Username = EncryptionService.AES_Encrypt(request.Username),
                UsernameHash = EncryptionService.HMAC_SHA256_Hash(request.UsernameHash),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.PasswordHash),
            });

            await context.SaveChangesAsync();

            return Results.Ok();
        });
    }
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string UsernameHash { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}