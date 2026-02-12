using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace EkaCare.SDK
{
    /// <summary>
    /// Main client for interacting with EkaCare API
    /// </summary>
    public class EkaCareClient : IDisposable
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly HttpClient _httpClient;
        private string? _accessToken;
        
        private const string DEFAULT_BASE_URL = "https://api.eka.care";

        public AuthService Auth { get; }
        public FileService Files { get; }
        public TranscriptionService Transcription { get; }

        /// <summary>
        /// Initialize EkaCare client with credentials
        /// </summary>
        /// <param name="clientId">Your client ID</param>
        /// <param name="clientSecret">Your client secret</param>
        /// <param name="baseUrl">Optional custom base URL</param>
        public EkaCareClient(string clientId, string clientSecret, string? baseUrl = null)
        {
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
            
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl ?? DEFAULT_BASE_URL),
                Timeout = TimeSpan.FromMinutes(5)
            };
            
            // Add User-Agent header to avoid CloudFront blocking
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "EkaCare-DotNet-SDK/1.0");

            Auth = new AuthService(_httpClient, _clientId, _clientSecret);
            Files = new FileService(_httpClient);
            Transcription = new TranscriptionService(_httpClient);
        }

        /// <summary>
        /// Set the access token for authenticated requests
        /// </summary>
        public void SetAccessToken(string accessToken)
        {
            _accessToken = accessToken;
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", accessToken);
        }

        /// <summary>
        /// Get the current access token
        /// </summary>
        public string? GetAccessToken() => _accessToken;

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
