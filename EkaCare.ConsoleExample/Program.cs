using EkaCare.SDK;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace EkaCare.ConsoleExample
{
    class Program
    {
        // --- Configuration - UPDATE THESE VALUES ---TODO: keep these values in .env and audio file path can be at run time.
        private const string CLIENT_ID = "<client_id>";
        private const string CLIENT_SECRET = "<client_secret>";
        private static readonly List<string> AUDIO_FILE_PATHS = new()
        {
            @"/Users/vickykumar/Downloads/non-vaded/12-dec-prescription.mp3",
        };
        private const string TEMPLATE_ID = "transcript_template"; // or "eka_emr_template" or "clinical_notes_template"

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== EkaCare Transcription Example ===\n");

            try
            {
                using var client = new EkaCareClient(CLIENT_ID, CLIENT_SECRET);

                // Step 1: Authenticate
                await AuthenticateAsync(client);

                // Step 2: Get Presigned URL
                var presignedUrl = await GetPresignedUrlAsync(client);
                Console.WriteLine($"Transaction ID: {presignedUrl.TxnId}\n");

                // Step 3: Upload Audio Files
                var uploadResults = await UploadAudioFilesAsync(client, presignedUrl);

                // Step 4: Initialize Transcription
                await InitializeTranscriptionAsync(client, presignedUrl, uploadResults);

                // Step 5: Poll for Results
                await PollTranscriptionResultsAsync(client, presignedUrl.TxnId);

                Console.WriteLine("\n=== Process Completed Successfully ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static async Task AuthenticateAsync(EkaCareClient client)
        {
            Console.WriteLine("=== Authentication ===");
            
            var tokenResponse = await client.Auth.LoginAsync();
            Console.WriteLine($"✓ Access Token: {tokenResponse.AccessToken[..20]}...");
            Console.WriteLine($"✓ Refresh Token: {tokenResponse.RefreshToken[..20]}...");
            Console.WriteLine($"✓ Expires In: {tokenResponse.ExpiresIn} seconds");
            Console.WriteLine($"✓ Refresh Expires In: {tokenResponse.RefreshExpiresIn} seconds");
            
            client.SetAccessToken(tokenResponse.AccessToken);
            
            // Optional: Refresh token example
            // When token expires (401 error), refresh it:
            // var refreshedTokens = await client.Auth.RefreshTokenAsync(
            //     tokenResponse.RefreshToken, 
            //     tokenResponse.AccessToken
            // );
            // client.SetAccessToken(refreshedTokens.AccessToken);
            
            Console.WriteLine("✓ Authentication successful\n");
        }

        static async Task<PresignedUrlResponse> GetPresignedUrlAsync(EkaCareClient client)
        {
            Console.WriteLine("=== Getting Presigned URL ===");
            
            var presignedUrl = await client.Files.GetPresignedUrlAsync("ekascribe-v2");
            
            Console.WriteLine($"✓ S3 URL: {presignedUrl.UploadData.Url}");
            Console.WriteLine($"✓ Folder Path: {presignedUrl.FolderPath}");
            Console.WriteLine($"✓ Transaction ID: {presignedUrl.TxnId}\n");
            
            return presignedUrl;
        }

        static async Task<List<UploadResult>> UploadAudioFilesAsync(
            EkaCareClient client,
            PresignedUrlResponse presignedUrl)
        {
            Console.WriteLine("=== Uploading Audio Files ===");
            
            var results = await client.Files.UploadFilesAsync(presignedUrl, AUDIO_FILE_PATHS);
            
            foreach (var result in results)
            {
                Console.WriteLine($"✓ Uploaded: {result.FileName}");
                Console.WriteLine($"  Key: {result.Key}");
                Console.WriteLine($"  Size: {result.Size:N0} bytes\n");
            }
            
            return results;
        }

        static async Task InitializeTranscriptionAsync(
            EkaCareClient client,
            PresignedUrlResponse presignedUrl,
            List<UploadResult> uploadResults)
        {
            Console.WriteLine("=== Initializing Transcription ===");
            
            var fileNames = uploadResults.ConvertAll(r => r.FileName);
            var batchS3Url = presignedUrl.UploadData.Url + presignedUrl.FolderPath;
            
            var request = new TransactionInitRequest
            {
                Mode = "dictation",
                Transfer = "non-vaded",
                BatchS3Url = batchS3Url,
                ClientGeneratedFiles = fileNames,
                ModelType = "pro",
                InputLanguage = new List<string> { "en-IN" },
                OutputLanguage = "en-IN",
                Speciality = "general_medicine",
                OutputFormatTemplate = new List<OutputFormatTemplate>
                {
                    new OutputFormatTemplate
                    {
                        TemplateId = TEMPLATE_ID,
                        CodificationNeeded = false
                    }
                },
                AdditionalData = new Dictionary<string, object>
                {
                    ["patient"] = new { name = "John Doe" },
                    ["mode"] = "dictation"
                }
            };
            
            var response = await client.Transcription.InitializeTransactionAsync(
                presignedUrl.TxnId,
                request);
            
            Console.WriteLine($"✓ Status: {response.Status}");
            Console.WriteLine($"✓ Message: {response.Message}");
            Console.WriteLine($"✓ Batch ID: {response.BId}\n");
        }

        static async Task PollTranscriptionResultsAsync(EkaCareClient client, string txnId)
        {
            Console.WriteLine("=== Polling for Transcription Results ===");
            
            var statusResponse = await client.Transcription.PollForCompletionAsync(
                txnId,
                maxDurationSeconds: 300,
                pollIntervalSeconds: 5);
            
            Console.WriteLine("\n=== Transcription Results ===");
            
            if (statusResponse?.Data?.Output != null)
            {
                foreach (var output in statusResponse.Data.Output)
                {
                    Console.WriteLine($"\nTemplate: {output.TemplateId}");
                    Console.WriteLine($"Status: {output.Status}");
                    Console.WriteLine($"Type: {output.Type}");
                    
                    if (output.Status == "success" && !string.IsNullOrEmpty(output.Value))
                    {
                        try
                        {
                            // Decode base64 value
                            var decodedBytes = Convert.FromBase64String(output.Value);
                            var decodedJson = System.Text.Encoding.UTF8.GetString(decodedBytes);
                            
                            Console.WriteLine("\nDecoded Result:");
                            var formattedJson = JsonSerializer.Serialize(
                                JsonSerializer.Deserialize<object>(decodedJson),
                                new JsonSerializerOptions { WriteIndented = true });
                            Console.WriteLine(formattedJson);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Could not decode result: {ex.Message}");
                            Console.WriteLine($"Raw value: {output.Value[..Math.Min(100, output.Value.Length)]}...");
                        }
                    }
                    
                    if (output.Errors?.Count > 0)
                    {
                        Console.WriteLine($"Errors: {string.Join(", ", output.Errors)}");
                    }
                    
                    if (output.Warnings?.Count > 0)
                    {
                        Console.WriteLine($"Warnings: {string.Join(", ", output.Warnings)}");
                    }
                }
            }
        }
    }
}
