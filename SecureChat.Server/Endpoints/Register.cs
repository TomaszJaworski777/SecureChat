using Microsoft.EntityFrameworkCore;

public static class RegisterEndpoints
{
    public static void MapRegisterEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/register", async (RegisterRequest request, DatabaseContext context) =>
        {
            if (await context.Users.CountAsync(x => x.Username == request.Username) > 0)
                return Results.Conflict();

            context.Users.Add(new User
            {
                Username = request.Username,
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
    public string PasswordHash { get; set; } = string.Empty;
}