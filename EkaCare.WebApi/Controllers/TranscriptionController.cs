using EkaCare.SDK;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EkaCare.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranscriptionController : ControllerBase
    {
        private readonly EkaCareClient _client;
        private readonly ILogger<TranscriptionController> _logger;

        public TranscriptionController(EkaCareClient client, ILogger<TranscriptionController> logger)
        {
            _client = client;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate and get access token
        /// </summary>
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthRequest? request = null)
        {
            try
            {
                EkaCare.SDK.EkaCareClient clientToUse;
                
                // If credentials provided in request, create new client
                if (request != null && !string.IsNullOrEmpty(request.ClientId) && !string.IsNullOrEmpty(request.ClientSecret))
                {
                    clientToUse = new EkaCare.SDK.EkaCareClient(request.ClientId, request.ClientSecret);
                }
                else
                {
                    // Use default client from DI
                    clientToUse = _client;
                }
                
                var tokenResponse = await clientToUse.Auth.LoginAsync();
                clientToUse.SetAccessToken(tokenResponse.AccessToken);
                
                // Store the client in session or return credentials info
                return Ok(new
                {
                    message = "Authentication successful",
                    access_token = tokenResponse.AccessToken,
                    refresh_token = tokenResponse.RefreshToken,
                    expires_in = tokenResponse.ExpiresIn,
                    refresh_expires_in = tokenResponse.RefreshExpiresIn
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken) || string.IsNullOrEmpty(request.AccessToken))
                {
                    return BadRequest(new { error = "Both refresh_token and access_token are required" });
                }

                var tokenResponse = await _client.Auth.RefreshTokenAsync(
                    request.RefreshToken, 
                    request.AccessToken
                );
                _client.SetAccessToken(tokenResponse.AccessToken);
                
                return Ok(new
                {
                    message = "Token refreshed successfully",
                    access_token = tokenResponse.AccessToken,
                    refresh_token = tokenResponse.RefreshToken,
                    expires_in = tokenResponse.ExpiresIn,
                    refresh_expires_in = tokenResponse.RefreshExpiresIn
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get presigned URL for file upload
        /// </summary>
        [HttpPost("presigned-url")]
        public async Task<IActionResult> GetPresignedUrl([FromQuery] string action = "ekascribe-v2", [FromBody] PresignedUrlRequest? request = null)
        {
            try
            {
                EkaCare.SDK.EkaCareClient clientToUse;
                
                // If credentials and token provided, create authenticated client
                if (request != null && !string.IsNullOrEmpty(request.ClientId) && 
                    !string.IsNullOrEmpty(request.ClientSecret) && !string.IsNullOrEmpty(request.AccessToken))
                {
                    clientToUse = new EkaCare.SDK.EkaCareClient(request.ClientId, request.ClientSecret);
                    clientToUse.SetAccessToken(request.AccessToken);
                }
                else
                {
                    // Use default client
                    clientToUse = _client;
                }
                
                var presignedUrl = await clientToUse.Files.GetPresignedUrlAsync(action);
                return Ok(presignedUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get presigned URL");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Upload audio files
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFiles([FromBody] UploadRequest request)
        {
            try
            {
                if (request.PresignedUrl == null || request.FilePaths == null || !request.FilePaths.Any())
                {
                    return BadRequest(new { error = "Invalid request data" });
                }

                var results = await _client.Files.UploadFilesAsync(
                    request.PresignedUrl,
                    request.FilePaths);

                return Ok(new
                {
                    message = "Files uploaded successfully",
                    results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File upload failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Initialize transcription transaction
        /// </summary>
        [HttpPost("initialize/{txnId}")]
        public async Task<IActionResult> InitializeTransaction(
            string txnId,
            [FromBody] TransactionInitRequestWrapper wrapper)
        {
            try
            {
                EkaCare.SDK.EkaCareClient clientToUse;
                
                // If credentials and token provided, create authenticated client
                if (!string.IsNullOrEmpty(wrapper.ClientId) && 
                    !string.IsNullOrEmpty(wrapper.ClientSecret) && !string.IsNullOrEmpty(wrapper.AccessToken))
                {
                    clientToUse = new EkaCare.SDK.EkaCareClient(wrapper.ClientId, wrapper.ClientSecret);
                    clientToUse.SetAccessToken(wrapper.AccessToken);
                }
                else
                {
                    // Use default client
                    clientToUse = _client;
                }
                
                var response = await clientToUse.Transcription.InitializeTransactionAsync(txnId, wrapper.Request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction initialization failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Upload file to S3 using presigned URL
        /// </summary>
        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadFile([FromBody] FileUploadRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.FilePath))
                {
                    return BadRequest(new { error = "File path is required" });
                }

                if (string.IsNullOrEmpty(request.UploadUrl))
                {
                    return BadRequest(new { error = "Upload URL is required" });
                }

                if (string.IsNullOrEmpty(request.FolderPath))
                {
                    return BadRequest(new { error = "Folder path is required" });
                }

                // Read the file
                if (!System.IO.File.Exists(request.FilePath))
                {
                    return BadRequest(new { error = $"File not found: {request.FilePath}" });
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(request.FilePath);
                var fileName = Path.GetFileName(request.FilePath);

                // Create presigned URL data structure
                var presignedUrlData = new EkaCare.SDK.PresignedUrlResponse
                {
                    TxnId = request.TxnId,
                    FolderPath = request.FolderPath,
                    UploadData = new EkaCare.SDK.UploadData
                    {
                        Url = request.UploadUrl,
                        Fields = request.Fields ?? new Dictionary<string, string>()
                    }
                };

                // Upload to S3
                var uploadResults = await _client.Files.UploadFilesAsync(
                    presignedUrlData,
                    new List<string> { request.FilePath });

                return Ok(new
                {
                    message = "File uploaded successfully",
                    fileName = fileName,
                    fileSize = fileBytes.Length,
                    uploadResults = uploadResults
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File upload failed");
                return StatusCode(500, new { error = ex.Message, details = ex.ToString() });
            }
        }

        /// <summary>
        /// Get transcription status
        /// </summary>
        [HttpPost("status/{txnId}")]
        public async Task<IActionResult> GetStatus(string txnId, [FromBody] StatusRequest? request = null)
        {
            try
            {
                EkaCare.SDK.EkaCareClient clientToUse;
                
                // If credentials and token provided, create authenticated client
                if (request != null && !string.IsNullOrEmpty(request.ClientId) && 
                    !string.IsNullOrEmpty(request.ClientSecret) && !string.IsNullOrEmpty(request.AccessToken))
                {
                    clientToUse = new EkaCare.SDK.EkaCareClient(request.ClientId, request.ClientSecret);
                    clientToUse.SetAccessToken(request.AccessToken);
                }
                else
                {
                    // Use default client
                    clientToUse = _client;
                }
                
                var status = await clientToUse.Transcription.GetStatusAsync(txnId);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get status");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Poll for transcription completion
        /// </summary>
        [HttpGet("poll/{txnId}")]
        public async Task<IActionResult> PollForCompletion(
            string txnId,
            [FromQuery] int maxDurationSeconds = 300,
            [FromQuery] int pollIntervalSeconds = 5)
        {
            try
            {
                var status = await _client.Transcription.PollForCompletionAsync(
                    txnId,
                    maxDurationSeconds,
                    pollIntervalSeconds);

                // Decode base64 results
                var decodedResults = new List<object>();
                
                if (status?.Data?.Output != null)
                {
                    foreach (var output in status.Data.Output)
                    {
                        object? decodedValue = null;
                        
                        if (output.Status == "success" && !string.IsNullOrEmpty(output.Value))
                        {
                            try
                            {
                                var decodedBytes = Convert.FromBase64String(output.Value);
                                var decodedJson = System.Text.Encoding.UTF8.GetString(decodedBytes);
                                decodedValue = JsonSerializer.Deserialize<object>(decodedJson);
                            }
                            catch
                            {
                                decodedValue = output.Value;
                            }
                        }
                        
                        decodedResults.Add(new
                        {
                            output.TemplateId,
                            output.Status,
                            output.Type,
                            output.Name,
                            DecodedValue = decodedValue,
                            output.Errors,
                            output.Warnings
                        });
                    }
                }

                return Ok(new
                {
                    message = "Transcription completed",
                    results = decodedResults
                });
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning(ex, "Polling timeout");
                return StatusCode(408, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Polling failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Complete workflow: authenticate, upload, transcribe
        /// </summary>
        [HttpPost("complete-workflow")]
        public async Task<IActionResult> CompleteWorkflow([FromBody] WorkflowRequest request)
        {
            try
            {
                // 1. Authenticate
                var tokenResponse = await _client.Auth.LoginAsync();
                _client.SetAccessToken(tokenResponse.AccessToken);

                // 2. Get presigned URL
                var presignedUrl = await _client.Files.GetPresignedUrlAsync("ekascribe-v2");

                // 3. Upload files
                var uploadResults = await _client.Files.UploadFilesAsync(
                    presignedUrl,
                    request.FilePaths);

                // 4. Initialize transaction
                var batchS3Url = presignedUrl.UploadData.Url + presignedUrl.FolderPath;
                var fileNames = uploadResults.ConvertAll(r => r.FileName);

                var initRequest = new TransactionInitRequest
                {
                    Mode = request.Mode ?? "dictation",
                    Transfer = "non-vaded",
                    BatchS3Url = batchS3Url,
                    ClientGeneratedFiles = fileNames,
                    ModelType = request.ModelType ?? "pro",
                    InputLanguage = request.InputLanguage ?? new List<string> { "en-IN" },
                    OutputLanguage = request.OutputLanguage ?? "en-IN",
                    Speciality = request.Speciality,
                    OutputFormatTemplate = request.OutputFormatTemplate ?? new List<OutputFormatTemplate>
                    {
                        new OutputFormatTemplate 
                        { 
                            TemplateId = "transcript_template",
                            TemplateType = "custom",
                            TemplateName = "Transcript Template"
                        }
                    }
                };

                var initResponse = await _client.Transcription.InitializeTransactionAsync(
                    presignedUrl.TxnId,
                    initRequest);

                // 5. Poll for results
                var status = await _client.Transcription.PollForCompletionAsync(
                    presignedUrl.TxnId,
                    request.MaxPollDurationSeconds ?? 300,
                    request.PollIntervalSeconds ?? 5);

                return Ok(new
                {
                    message = "Workflow completed successfully",
                    txnId = presignedUrl.TxnId,
                    uploadResults,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Workflow failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Request models
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
    }

    public class UploadRequest
    {
        public PresignedUrlResponse? PresignedUrl { get; set; }
        public List<string>? FilePaths { get; set; }
    }

    public class WorkflowRequest
    {
        public List<string> FilePaths { get; set; } = new();
        public string? Mode { get; set; }
        public string? ModelType { get; set; }
        public List<string>? InputLanguage { get; set; }
        public string? OutputLanguage { get; set; }
        public string? Speciality { get; set; }
        public List<OutputFormatTemplate>? OutputFormatTemplate { get; set; }
        public int? MaxPollDurationSeconds { get; set; }
        public int? PollIntervalSeconds { get; set; }
    }

    public class FileUploadRequest
    {
        public string FilePath { get; set; } = string.Empty;
        public string TxnId { get; set; } = string.Empty;
        public string UploadUrl { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public Dictionary<string, string>? Fields { get; set; }
    }

    public class AuthRequest
    {
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
    }

    public class PresignedUrlRequest
    {
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? AccessToken { get; set; }
    }

    public class TransactionInitRequestWrapper
    {
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? AccessToken { get; set; }
        public TransactionInitRequest Request { get; set; } = new();
    }

    public class StatusRequest
    {
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? AccessToken { get; set; }
    }
}
