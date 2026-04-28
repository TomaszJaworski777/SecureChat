using Microsoft.EntityFrameworkCore;

public static class LoginEndpoints {
    public static void MapLoginEndpoints(this IEndpointRouteBuilder app) {
        app.MapPost("/login", async (LoginRequest request, DatabaseContext context) =>
        {
            var usernameHash = EncryptionService.HMAC_SHA256_Hash(request.UsernameHash);

            var user = await context.Users
                .FirstOrDefaultAsync(x => x.UsernameHash == usernameHash);

            if (user is null)
                return Results.NotFound();

            if (!BCrypt.Net.BCrypt.Verify(request.PasswordHash, user.PasswordHash))
                return Results.Unauthorized();

            return Results.Ok(); // TODO: return auth token
        });

        app.MapGet("/users", (DatabaseContext context) =>
        {
            return Results.Ok(context.Users.ToList());
        });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string UsernameHash { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}
