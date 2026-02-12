using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Linq;

namespace EkaCare.SDK
{
    public class TranscriptionService
    {
        private readonly HttpClient _httpClient;

        public TranscriptionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Initialize transcription transaction
        /// </summary>
        public async Task<TransactionInitResponse> InitializeTransactionAsync(
            string txnId,
            TransactionInitRequest request)
        {
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            // Debug: Log the request JSON
            Console.WriteLine($"[DEBUG] Initialize Request JSON: {json}");
            Console.WriteLine($"[DEBUG] BatchS3Url value: {request.BatchS3Url}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"/voice/api/v2/transaction/init/{txnId}", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Initialize transaction failed with status {response.StatusCode}. " +
                    $"Response: {errorContent}. " +
                    $"Request URL: {response.RequestMessage?.RequestUri}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var initResponse = JsonSerializer.Deserialize<TransactionInitResponse>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return initResponse ?? throw new Exception("Failed to initialize transaction");
        }

        /// <summary>
        /// Get transcription status and results
        /// </summary>
        public async Task<TranscriptionStatusResponse> GetStatusAsync(string txnId)
        {
            var response = await _httpClient.GetAsync($"/voice/api/v3/status/{txnId}");
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var statusResponse = JsonSerializer.Deserialize<TranscriptionStatusResponse>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return statusResponse ?? throw new Exception("Failed to get status");
        }

        /// <summary>
        /// Poll for transcription completion with timeout
        /// </summary>
        public async Task<TranscriptionStatusResponse> PollForCompletionAsync(
            string txnId,
            int maxDurationSeconds = 300,
            int pollIntervalSeconds = 5)
        {
            var startTime = DateTime.UtcNow;
            var maxDuration = TimeSpan.FromSeconds(maxDurationSeconds);

            while (true)
            {
                var elapsed = DateTime.UtcNow - startTime;
                if (elapsed >= maxDuration)
                {
                    throw new TimeoutException($"Polling timeout after {maxDurationSeconds} seconds");
                }

                Console.WriteLine($"Polling status... (elapsed: {elapsed.TotalSeconds:F1}s)");

                try
                {
                    var status = await GetStatusAsync(txnId);
                    
                    // Check if transcription is complete
                    if (status?.Data?.Output != null && status.Data.Output.Any())
                    {
                        var allComplete = status.Data.Output.All(o => 
                            o.Status?.Equals("success", StringComparison.OrdinalIgnoreCase) == true ||
                            o.Status?.Equals("failed", StringComparison.OrdinalIgnoreCase) == true);

                        if (allComplete)
                        {
                            Console.WriteLine("Transcription completed!");
                            return status;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during polling: {ex.Message}");
                }

                Console.WriteLine($"Waiting {pollIntervalSeconds} seconds before next poll...");
                await Task.Delay(TimeSpan.FromSeconds(pollIntervalSeconds));
            }
        }
    }

    // Request/Response Models
    public class TransactionInitRequest
    {
        [JsonPropertyName("mode")]
        public string Mode { get; set; } = "dictation";

        [JsonPropertyName("transfer")]
        public string Transfer { get; set; } = "non-vaded";

        [JsonPropertyName("batch_s3_url")]
        public string BatchS3Url { get; set; } = string.Empty;

        [JsonPropertyName("client_generated_files")]
        public List<string> ClientGeneratedFiles { get; set; } = new();

        [JsonPropertyName("model_type")]
        public string ModelType { get; set; } = "pro";

        [JsonPropertyName("input_language")]
        public List<string> InputLanguage { get; set; } = new() { "en-IN" };

        [JsonPropertyName("output_language")]
        public string OutputLanguage { get; set; } = "en-IN";

        [JsonPropertyName("speciality")]
        public string? Speciality { get; set; }

        [JsonPropertyName("output_format_template")]
        public List<OutputFormatTemplate> OutputFormatTemplate { get; set; } = new();

        [JsonPropertyName("additional_data")]
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    public class OutputFormatTemplate
    {
        [JsonPropertyName("template_id")]
        public string TemplateId { get; set; } = string.Empty;

        [JsonPropertyName("codification_needed")]
        public bool CodificationNeeded { get; set; } = false;
    }

    public class TransactionInitResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("txn_id")]
        public string TxnId { get; set; } = string.Empty;

        [JsonPropertyName("b_id")]
        public string BId { get; set; } = string.Empty;
    }

    public class TranscriptionStatusResponse
    {
        [JsonPropertyName("data")]
        public TranscriptionData? Data { get; set; }
    }

    public class TranscriptionData
    {
        [JsonPropertyName("output")]
        public List<TranscriptionOutput> Output { get; set; } = new();

        [JsonPropertyName("additional_data")]
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    public class TranscriptionOutput
    {
        [JsonPropertyName("template_id")]
        public string TemplateId { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = new();

        [JsonPropertyName("warnings")]
        public List<string> Warnings { get; set; } = new();
    }
}
