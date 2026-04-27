using Microsoft.EntityFrameworkCore;

public static class LoginEndpoints {
    public static void MapLoginEndpoints(this IEndpointRouteBuilder app) {
        app.MapPost("/login", async (LoginRequest request, DatabaseContext context) =>
        {
            var user = await context.Users
                .FirstOrDefaultAsync(x => x.Username == request.Username);

            if (user is null)
                return Results.NotFound();

            if (!BCrypt.Net.BCrypt.Verify(request.PasswordHash, user.PasswordHash))
                return Results.Unauthorized();

            return Results.Ok(); // TODO: return auth token
        });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}
