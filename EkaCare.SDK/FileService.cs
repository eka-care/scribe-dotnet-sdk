using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EkaCare.SDK
{
    public class FileService
    {
        private readonly HttpClient _httpClient;

        public FileService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Get presigned URL for file upload
        /// </summary>
        /// <param name="action">Action type (e.g., "ekascribe-v2")</param>
        public async Task<PresignedUrlResponse> GetPresignedUrlAsync(string action = "ekascribe-v2")
        {
            var response = await _httpClient.PostAsync($"/v1/file-upload?action={action}", null);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var presignedResponse = JsonSerializer.Deserialize<PresignedUrlResponse>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return presignedResponse ?? throw new Exception("Failed to get presigned URL");
        }

        /// <summary>
        /// Upload audio files to S3 using presigned URL
        /// </summary>
        public async Task<List<UploadResult>> UploadFilesAsync(
            PresignedUrlResponse presignedUrl,
            List<string> filePaths)
        {
            var results = new List<UploadResult>();

            foreach (var filePath in filePaths)
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                var result = await UploadSingleFileAsync(presignedUrl, filePath);
                results.Add(result);
            }

            return results;
        }

        private async Task<UploadResult> UploadSingleFileAsync(
            PresignedUrlResponse presignedUrl,
            string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var fileSize = new FileInfo(filePath).Length;

            using var form = new MultipartFormDataContent();

            // Add all fields from presigned URL
            var fields = presignedUrl.UploadData.Fields;
            
            // Update the key with actual filename
            var key = presignedUrl.FolderPath + fileName;
            form.Add(new StringContent(key), "key");

            // Add other fields
            foreach (var field in fields)
            {
                if (field.Key != "key") // Skip key as we already added it
                {
                    form.Add(new StringContent(field.Value), field.Key);
                }
            }

            // Add the file - MUST be last
            var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            form.Add(fileContent, "file", fileName);

            // Upload to S3
            using var s3Client = new HttpClient();
            var response = await s3Client.PostAsync(presignedUrl.UploadData.Url, form);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent || 
                response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new UploadResult
                {
                    Key = key,
                    FileName = fileName,
                    Size = fileSize,
                    Success = true
                };
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Upload failed for {fileName}: {response.StatusCode} - {errorContent}");
        }
    }

    public class PresignedUrlResponse
    {
        [JsonPropertyName("uploadData")]
        public UploadData UploadData { get; set; } = new();

        [JsonPropertyName("folderPath")]
        public string FolderPath { get; set; } = string.Empty;

        [JsonPropertyName("txn_id")]
        public string TxnId { get; set; } = string.Empty;
    }

    public class UploadData
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("fields")]
        public Dictionary<string, string> Fields { get; set; } = new();
    }

    public class UploadResult
    {
        public string Key { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public bool Success { get; set; }
    }
}
