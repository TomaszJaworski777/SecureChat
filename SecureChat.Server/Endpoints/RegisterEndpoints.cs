using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public static class RegisterEndpoints
{
    public static void MapRegisterEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/register", async (RegisterRequest request, DatabaseContext context) =>
        {
            var usernameHash = EncryptionService.HMAC_SHA256_Hash(request.UsernameHash);

            if (await context.Users.CountAsync(x => x.UsernameHash == usernameHash) > 0)
                return Results.Conflict();

            var user = new User
            {
                Username = EncryptionService.AES_Encrypt(request.Username),
                UsernameHash = EncryptionService.HMAC_SHA256_Hash(request.UsernameHash),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.PasswordHash),
                PublicKey = EncryptionService.AES_Encrypt(request.PublicKey),
                CreationDate = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

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
            return Results.Ok(new { token });
        }).WithName("PostRegister");
    }
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string UsernameHash { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
}
