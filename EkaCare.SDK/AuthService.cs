using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EkaCare.SDK
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public AuthService(HttpClient httpClient, string clientId, string clientSecret)
        {
            _httpClient = httpClient;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        /// <summary>
        /// Login and get access token
        /// POST https://api.eka.care/connect-auth/v1/account/login
        /// </summary>
        public async Task<TokenResponse> LoginAsync(string? sharingKey = null)
        {
            var loginData = new
            {
                client_id = _clientId,
                client_secret = _clientSecret,
                sharing_key = sharingKey ?? ""
            };

            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/connect-auth/v1/account/login", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Authentication failed with status {response.StatusCode}. " +
                    $"Response: {errorContent}. " +
                    $"Request URL: {response.RequestMessage?.RequestUri}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return tokenResponse ?? throw new Exception("Failed to deserialize token response");
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// POST https://api.eka.care/connect-auth/v1/account/refresh-token
        /// Requires Authorization header with current access token
        /// </summary>
        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, string accessToken)
        {
            var refreshData = new
            {
                refresh_token = refreshToken,
                access_token = accessToken
            };

            var json = JsonSerializer.Serialize(refreshData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Create request with required headers
            var request = new HttpRequestMessage(HttpMethod.Post, "/connect-auth/v1/account/refresh-token")
            {
                Content = content
            };
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            request.Headers.Add("Client-Id", _clientId);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return tokenResponse ?? throw new Exception("Failed to deserialize token response");
        }
    }

    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_expires_in")]
        public int RefreshExpiresIn { get; set; }
    }
}
