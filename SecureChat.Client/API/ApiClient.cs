using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace SecureChat.Client.API
{
    public class ApiClient
    {
        private class Authentication
        {
            public string token { get; set; } = "";
        }

        private class Username
        {
            public string username { get; set; } = "";
        }

        public class Contact
        {
            [JsonPropertyName("id")]
            public int ID { get; set; }
            [JsonPropertyName("username")]
            public string Username { get; set; } = "";
            [JsonPropertyName("lastMessage")]
            public string LastMessage { get; set; } = "";
            [JsonPropertyName("lastMessageDate")]
            public DateTime LastMessageDate { get; set; }
            [JsonPropertyName("isOnline")]
            public bool IsOnline { get; set; }
        }

        public class Message
        {
            [JsonPropertyName("id")]
            public int ID { get; set; }
            [JsonPropertyName("senderId")]
            public int SenderID { get; set; }
            [JsonPropertyName("senderUsername")]
            public string SenderUsername { get; set; } = "";
            [JsonPropertyName("recieverUsername")]
            public string RecieverUsername { get; set; } = "";
            [JsonPropertyName("content")]
            public string Content { get; set; } = "";
            [JsonPropertyName("date")]
            public DateTime Date { get; set; }
        }

        private readonly HttpClient _http = new() { BaseAddress = new Uri("http://localhost:5000") };

        private string _authenticationToken = "";

        private string _login = "";
        private string _password = "";

        public async Task<HttpStatusCode> LoginAsync(string username, string password)
        {
            var response = await _http.PostAsJsonAsync("/login", new { Username = username, UsernameHash = HashString(username), PasswordHash = HashString(password) });

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<Authentication>();
                _authenticationToken = authResponse is null ? "" : authResponse.token;

                _login = username;
                _password = password;
                _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authenticationToken);
            }

            return response.StatusCode;
        }

        public async Task<HttpStatusCode> RegisterAsync(string username, string password)
        {
            var response = await _http.PostAsJsonAsync("/register", new { Username = username, UsernameHash = HashString(username), PasswordHash = HashString(password) });

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<Authentication>();
                _authenticationToken = authResponse is null ? "" : authResponse.token;

                _login = username;
                _password = password;
                _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authenticationToken);
            }

            return response.StatusCode;
        }

        public async Task<string> GetUsernameAsync()
        {
            var response = await ResyncGet("/username");

            var usernameResponse = await response.Content.ReadFromJsonAsync<Username>();
            var username = usernameResponse is null ? "ERROR - " + response.StatusCode : usernameResponse.username;

            return username;
        }

        public async Task<List<Contact>> GetContactsAsync()
        {
            var response = await ResyncGet("/contacts");

            var contactsReponse = await response.Content.ReadFromJsonAsync<List<Contact>>();
            var contacts = contactsReponse is null ? new List<Contact>() : contactsReponse;

            return contacts;
        }

        public async Task<List<Message>> GetMessagesAsync()
        {
            var response = await ResyncGet("/messages");

            var messagesReponse = await response.Content.ReadFromJsonAsync<List<Message>>();
            var messages = messagesReponse is null ? new List<Message>() : messagesReponse;

            return messages;
        }

        public async Task<List<Message>> GetMessagesAsync(int targetId)
        {
            var response = await ResyncGet("/messages/" + targetId);

            var messagesReponse = await response.Content.ReadFromJsonAsync<List<Message>>();
            var messages = messagesReponse is null ? new List<Message>() : messagesReponse;

            return messages;
        }

        private async Task<HttpResponseMessage> ResyncGet(string endpoint) {
            var response = await _http.GetAsync(endpoint);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if ((await LoginAsync(_login, _password)) == HttpStatusCode.OK)
                    return await ResyncGet(endpoint);
            }

            return response;
        }

        private string HashString(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}

