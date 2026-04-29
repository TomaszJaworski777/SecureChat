using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                ]),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(EncryptionService.JWT_Key),
                    SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
            return Results.Ok(new { id = user.Id, token });
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
