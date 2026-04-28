using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace SecureChat.Client.API
{
    public class Auth
    {
        private readonly HttpClient _http = new() { BaseAddress = new Uri("http://localhost:5000") };

        public async Task<HttpStatusCode> LoginAsync(string username, string password)
        {
            var response = await _http.PostAsJsonAsync("/login", new { Username = username, UsernameHash = HashString(username), PasswordHash = HashString(password) });
            return response.StatusCode;
        }

        public async Task<HttpStatusCode> RegisterAsync(string username, string password)
        {
            var response = await _http.PostAsJsonAsync("/register", new { Username = username, UsernameHash = HashString(username), PasswordHash = HashString(password) });
            return response.StatusCode;
        }

        private string HashString(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}

