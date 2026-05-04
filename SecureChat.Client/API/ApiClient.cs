using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
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
            public int id { get; set; }
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
            [JsonPropertyName("publicKey")]
            public string PublicKey { get; set; } = "";
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

        public string PrivateKey { get; set; } = "";
        public string PublicKey { get; set; } = "";

        private readonly HttpClient _http = new() { BaseAddress = new Uri("http://localhost:5000") };
        private HubConnection? _hub;

        private string _authenticationToken = "";
        private DateTime _tokenExpiry = DateTime.MinValue;

        private string _username = "";
        private string _password = "";

        private Action? _disconnectCallback;

        public async Task ResetAsync() {
            _http.DefaultRequestHeaders.Authorization = null;

            if (_hub != null)
            {
                await _hub.DisposeAsync();
                _hub = null;
            }

            _authenticationToken = "";
            _username = "";
            _password = "";

            _disconnectCallback = null;
        }

        #region Endpoints
        public async Task<HttpStatusCode> LoginAsync(string username, string password)
        {
            var response = await _http.PostAsJsonAsync("/login", new { Username = username, UsernameHash = HashString(username), PasswordHash = HashString(password) });

            if (response is null)
                throw new NullReferenceException();

            if (response.StatusCode == HttpStatusCode.OK)
                await ProcessLoginAsync(response, username, password);

            return response.StatusCode;
        }

        public async Task<HttpStatusCode> RegisterAsync(string username, string password)
        {
            GenerateMessageKeys(username, password);

            var response = await _http.PostAsJsonAsync("/register", new { Username = username, UsernameHash = HashString(username), PasswordHash = HashString(password), PublicKey = PublicKey });

            if (response.StatusCode == HttpStatusCode.OK)
                await ProcessLoginAsync(response, username, password);

            return response.StatusCode;
        }

        private async Task ProcessLoginAsync(HttpResponseMessage response, string username, string password) {
            var authResponse = await response.Content.ReadFromJsonAsync<Authentication>();
            _authenticationToken = authResponse is null ? "" : authResponse.token;
            _tokenExpiry = DateTime.UtcNow.AddMinutes(5);

            _username = username;
            _password = password;
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authenticationToken);

            GenerateMessageKeys(username, password);

            _hub = new HubConnectionBuilder().WithUrl("http://localhost:5000/events", options =>
            {
                options.AccessTokenProvider = GetFreshTokenAsync;
            }).Build();

            _hub.Closed += async (error) =>
            {
                _disconnectCallback?.Invoke();
                await Task.CompletedTask;
            };

            try
            {
                await _hub.StartAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public async Task<string> GetUsernameAsync()
        {
            var response = await ResyncGet("/username");

            if (response.StatusCode != HttpStatusCode.OK)
                return string.Format($"ERROR - {response.StatusCode}");

            var usernameResponse = await response.Content.ReadFromJsonAsync<Username>();
            var username = usernameResponse is null ? "ERROR - " + response.StatusCode : usernameResponse.username;

            return username;
        }

        public async Task<List<Contact>> GetContactsAsync()
        {
            var response = await ResyncGet("/contacts");

            if (response.StatusCode != HttpStatusCode.OK)
                return new List<Contact>();

            var contactsReponse = await response.Content.ReadFromJsonAsync<List<Contact>>();
            var contacts = contactsReponse is null ? new List<Contact>() : contactsReponse;

            return contacts;
        }

        public async Task<List<Message>> GetMessagesAsync()
        {
            var response = await ResyncGet("/messages");

            if (response.StatusCode != HttpStatusCode.OK)
                return new List<Message>();

            var messagesReponse = await response.Content.ReadFromJsonAsync<List<Message>>();
            var messages = messagesReponse is null ? new List<Message>() : messagesReponse;

            return messages;
        }

        public async Task<List<Message>> GetMessagesAsync(int targetId)
        {
            var response = await ResyncGet("/messages/" + targetId);

            if (response.StatusCode != HttpStatusCode.OK)
                return new List<Message>();

            var messagesReponse = await response.Content.ReadFromJsonAsync<List<Message>>();
            var messages = messagesReponse is null ? new List<Message>() : messagesReponse;

            return messages;
        }

        private async Task<HttpResponseMessage> ResyncGet(string endpoint) {
            var response = await _http.GetAsync(endpoint);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if ((await LoginAsync(_username, _password)) == HttpStatusCode.OK)
                    return await ResyncGet(endpoint);
            }

            return response;
        }

        private async Task<string?> GetFreshTokenAsync()
        {
            if (DateTime.UtcNow < _tokenExpiry.AddSeconds(-30))
                return _authenticationToken;

            var response = await _http.PostAsJsonAsync("/login", new {
                Username = _username,
                UsernameHash = HashString(_username),
                PasswordHash = HashString(_password)
            });

            if (response.StatusCode != HttpStatusCode.OK)
                return _authenticationToken;

            var auth = await response.Content.ReadFromJsonAsync<Authentication>();
            if (auth is null)
                return _authenticationToken;

            _authenticationToken = auth.token;
            _tokenExpiry = DateTime.UtcNow.AddMinutes(5);
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authenticationToken);

            return _authenticationToken;
        }

        private string HashString(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes).ToLower();
        }

        private void GenerateMessageKeys( string username, string password ) {
            var salt = Encoding.UTF8.GetBytes(username);
            var seedKey = SHA256.HashData(Encoding.UTF8.GetBytes(username + password + salt.Length));
            var seed = Rfc2898DeriveBytes.Pbkdf2(
                    seedKey,
                    salt,
                    iterations: 600_000,
                    HashAlgorithmName.SHA256,
                    outputLength: 32
                );

            var ecdh = ECDiffieHellman.Create(new ECParameters { 
                Curve = ECCurve.NamedCurves.nistP256,
                D = seed,
            });

            PublicKey = Convert.ToBase64String(ecdh.PublicKey.ExportSubjectPublicKeyInfo());
            PrivateKey = Convert.ToBase64String(ecdh.ExportPkcs8PrivateKey());
        }
        #endregion

        #region Events
        public async Task SendMessageAsync(Contact receiver, string message) {
            var privateEcdh = ECDiffieHellman.Create();
            privateEcdh.ImportPkcs8PrivateKey(Convert.FromBase64String(PrivateKey), out _);

            var publicEcdh = ECDiffieHellman.Create();
            publicEcdh.ImportSubjectPublicKeyInfo(Convert.FromBase64String(receiver.PublicKey), out _);

            var sharedSecret = privateEcdh.DeriveKeyMaterial(publicEcdh.PublicKey);

            using var aes = new AesGcm(sharedSecret, AesGcm.TagByteSizes.MaxSize);
            var iv = RandomNumberGenerator.GetBytes(12);
            var plain = Encoding.UTF8.GetBytes(message);
            var cipher = new byte[plain.Length];
            var tag = new byte[16];
            aes.Encrypt(iv, plain, cipher, tag);

            var encrypedMessage = Convert.ToBase64String([.. iv, .. tag, .. cipher]);

            if (_hub is null)
                return;

            try
            {
                await _hub!.InvokeAsync("SendMessage", receiver.ID, encrypedMessage);
            }
            catch (Exception e) {
                Debug.WriteLine(e);
            }
        }

        public void RegisterReceiveMessageCallback(Action<Message> callback) {
            if (_hub is null)
                return;

            _hub.On("ReceiveMessage", callback);
        }

        public void RegisterUserOnlineCallback(Action<int, bool> callback)
        {
            if (_hub is null)
                return;

            _hub.On("UserOnlineState", callback);
        }

        public void RegisterNewUserCreatedCallback(Action<Contact> callback)
        {
            if (_hub is null)
                return;

            _hub.On("NewUserCreated", callback);
        }

        public void RegisterForceDisconnectCallback(Action callback)
        {
            if (_hub is null) 
                return;

            _disconnectCallback += callback;
        }
        #endregion
    }
}

