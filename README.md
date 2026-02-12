# EkaCare .NET SDK

A comprehensive .NET SDK for the EkaCare Medical Transcription API, supporting audio transcription with multiple templates and languages.

## 📋 Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Running Locally](#running-locally)
- [Running Online](#running-online)
- [API Reference](#api-reference)
- [Examples](#examples)
- [Configuration](#configuration)
- [Troubleshooting](#troubleshooting)

## ✨ Features

- **Authentication**: OAuth2 client credentials flow with token refresh
- **File Upload**: Direct S3 upload using presigned URLs
- **Transcription**: Medical audio transcription with multiple templates
- **Polling**: Automatic status polling with timeout handling
- **Templates**: Support for transcript, EMR, and clinical notes templates
- **Multi-language**: Support for 12+ Indian and international languages
- **Async/Await**: Full async support for .NET applications

## 📦 Prerequisites

- **.NET 8.0 SDK** or later
- **Visual Studio 2022** / **VS Code** / **JetBrains Rider**
- **EkaCare API Credentials** (Client ID and Secret)
- **Audio files** for transcription (WAV, MP3, M4A formats)

### Installing .NET 8.0

**Windows:**
```bash
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0
# Or use winget:
winget install Microsoft.DotNet.SDK.8
```

**macOS:**
```bash
brew install dotnet-sdk
```

**Linux (Ubuntu):**
```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0
```

**Verify Installation:**
```bash
dotnet --version
# Should output: 8.0.x
```

## 🚀 Installation

### Option 1: Clone this Repository

```bash
git clone https://github.com/yourusername/ekacare-dotnet-sdk.git
cd ekacare-dotnet-sdk/EkaCare.Solution
```

### Option 2: Add SDK to Existing Project

```bash
# If the SDK is published to NuGet
dotnet add package EkaCare.SDK

# Or reference the project directly
dotnet add reference path/to/EkaCare.SDK/EkaCare.SDK.csproj
```

## 🎯 Quick Start

### Console Application

```csharp
using EkaCare.SDK;

var client = new EkaCareClient("your_client_id", "your_client_secret");

// Authenticate
var token = await client.Auth.LoginAsync();
client.SetAccessToken(token.AccessToken);

// Get presigned URL
var presignedUrl = await client.Files.GetPresignedUrlAsync();

// Upload files
var results = await client.Files.UploadFilesAsync(
    presignedUrl, 
    new List<string> { "path/to/audio.wav" });

// Initialize transcription
var request = new TransactionInitRequest
{
    Mode = "dictation",
    BatchS3Url = presignedUrl.UploadData.Url + presignedUrl.FolderPath,
    ClientGeneratedFiles = results.Select(r => r.FileName).ToList(),
    OutputFormatTemplate = new List<OutputFormatTemplate>
    {
        new() { TemplateId = "transcript_template" }
    }
};

await client.Transcription.InitializeTransactionAsync(presignedUrl.TxnId, request);

// Poll for results
var status = await client.Transcription.PollForCompletionAsync(presignedUrl.TxnId);
```

## 🏃 Running Locally

### Method 1: Console Application

1. **Navigate to Console Example:**
   ```bash
   cd EkaCare.Solution/EkaCare.ConsoleExample
   ```

2. **Update Configuration:**
   Edit `Program.cs` and update:
   ```csharp
   private const string CLIENT_ID = "your_actual_client_id";
   private const string CLIENT_SECRET = "your_actual_client_secret";
   private static readonly List<string> AUDIO_FILE_PATHS = new()
   {
       @"C:\path\to\your\audio1.wav",
       @"C:\path\to\your\audio2.mp3"
   };
   ```

3. **Run the Application:**
   ```bash
   dotnet run
   ```

### Method 2: Web API

1. **Navigate to Web API:**
   ```bash
   cd EkaCare.Solution/EkaCare.WebApi
   ```

2. **Update Configuration:**
   Edit `appsettings.json`:
   ```json
   {
     "EkaCare": {
       "ClientId": "your_actual_client_id",
       "ClientSecret": "your_actual_client_secret"
     }
   }
   ```

3. **Run the API:**
   ```bash
   dotnet run
   ```

4. **Access Swagger UI:**
   Open browser to: `https://localhost:5001/swagger`

5. **Test with cURL:**
   ```bash
   # Authenticate
   curl -X POST https://localhost:5001/api/transcription/authenticate
   
   # Get presigned URL
   curl -X POST https://localhost:5001/api/transcription/presigned-url
   
   # Complete workflow
   curl -X POST https://localhost:5001/api/transcription/complete-workflow \
     -H "Content-Type: application/json" \
     -d '{
       "filePaths": ["C:\\path\\to\\audio.wav"],
       "mode": "dictation",
       "modelType": "pro",
       "outputFormatTemplate": [
         {"templateId": "transcript_template"}
       ]
     }'
   ```

### Method 3: Visual Studio

1. **Open Solution:**
   - Launch Visual Studio 2022
   - Open `EkaCare.Solution/EkaCare.sln`

2. **Set Startup Project:**
   - Right-click `EkaCare.ConsoleExample` or `EkaCare.WebApi`
   - Select "Set as Startup Project"

3. **Update Configuration** (as described above)

4. **Press F5** to run with debugging

## ☁️ Running Online

### Option 1: Azure App Service

1. **Publish from Visual Studio:**
   ```
   Right-click EkaCare.WebApi → Publish → Azure → App Service
   ```

2. **Or use Azure CLI:**
   ```bash
   az webapp up --name ekacare-api --resource-group MyResourceGroup
   ```

3. **Set Environment Variables:**
   ```bash
   az webapp config appsettings set \
     --name ekacare-api \
     --resource-group MyResourceGroup \
     --settings EkaCare__ClientId="your_id" EkaCare__ClientSecret="your_secret"
   ```

### Option 2: Docker

1. **Create Dockerfile:**
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
   WORKDIR /src
   COPY . .
   RUN dotnet restore
   RUN dotnet publish -c Release -o /app
   
   FROM mcr.microsoft.com/dotnet/aspnet:8.0
   WORKDIR /app
   COPY --from=build /app .
   ENTRYPOINT ["dotnet", "EkaCare.WebApi.dll"]
   ```

2. **Build and Run:**
   ```bash
   docker build -t ekacare-api .
   docker run -p 5000:8080 \
     -e EkaCare__ClientId="your_id" \
     -e EkaCare__ClientSecret="your_secret" \
     ekacare-api
   ```

### Option 3: GitHub Codespaces

1. **Fork this repository**
2. **Click "Code" → "Codespaces" → "Create codespace"**
3. **Update configuration files**
4. **Run:**
   ```bash
   cd EkaCare.Solution/EkaCare.WebApi
   dotnet run
   ```

### Option 4: Replit

1. **Import from GitHub** to Replit
2. **Set Secrets:**
   - `EKACARE_CLIENT_ID`
   - `EKACARE_CLIENT_SECRET`
3. **Click "Run"**

## 📚 API Reference

### EkaCareClient

```csharp
var client = new EkaCareClient(
    clientId: "your_client_id",
    clientSecret: "your_client_secret",
    baseUrl: "https://api.eka.care" // Optional
);
```

### Authentication

```csharp
// Login
var token = await client.Auth.LoginAsync();
client.SetAccessToken(token.AccessToken);

// Refresh token
var refreshed = await client.Auth.RefreshTokenAsync(token.RefreshToken);
client.SetAccessToken(refreshed.AccessToken);
```

### File Operations

```csharp
// Get presigned URL
var presignedUrl = await client.Files.GetPresignedUrlAsync("ekascribe-v2");

// Upload files
var results = await client.Files.UploadFilesAsync(
    presignedUrl,
    new List<string> { "audio1.wav", "audio2.mp3" }
);
```

### Transcription

```csharp
// Initialize transaction
var request = new TransactionInitRequest
{
    Mode = "dictation",
    Transfer = "non-vaded",
    BatchS3Url = presignedUrl.UploadData.Url + presignedUrl.FolderPath,
    ClientGeneratedFiles = new List<string> { "audio.wav" },
    ModelType = "pro",
    InputLanguage = new List<string> { "en-IN" },
    OutputLanguage = "en-IN",
    Speciality = "general_medicine",
    OutputFormatTemplate = new List<OutputFormatTemplate>
    {
        new() { TemplateId = "transcript_template", CodificationNeeded = false }
    }
};

var initResponse = await client.Transcription.InitializeTransactionAsync(
    presignedUrl.TxnId,
    request
);

// Get status
var status = await client.Transcription.GetStatusAsync(presignedUrl.TxnId);

// Poll for completion
var finalStatus = await client.Transcription.PollForCompletionAsync(
    presignedUrl.TxnId,
    maxDurationSeconds: 300,
    pollIntervalSeconds: 5
);
```

## 📖 Examples

### Example 1: Simple Transcription

```csharp
using EkaCare.SDK;

var client = new EkaCareClient("client_id", "client_secret");

// Authenticate
var token = await client.Auth.LoginAsync();
client.SetAccessToken(token.AccessToken);

// Get presigned URL
var presignedUrl = await client.Files.GetPresignedUrlAsync();

// Upload file
var uploads = await client.Files.UploadFilesAsync(
    presignedUrl,
    new List<string> { "consultation.wav" }
);

// Start transcription
var request = new TransactionInitRequest
{
    Mode = "dictation",
    BatchS3Url = presignedUrl.UploadData.Url + presignedUrl.FolderPath,
    ClientGeneratedFiles = uploads.Select(u => u.FileName).ToList(),
    OutputFormatTemplate = new()
    {
        new() { TemplateId = "transcript_template" }
    }
};

await client.Transcription.InitializeTransactionAsync(presignedUrl.TxnId, request);

// Wait for results
var result = await client.Transcription.PollForCompletionAsync(presignedUrl.TxnId);

// Decode results
foreach (var output in result.Data.Output)
{
    if (output.Status == "success")
    {
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(output.Value));
        Console.WriteLine(decoded);
    }
}
```

### Example 2: Multiple Languages

```csharp
var request = new TransactionInitRequest
{
    Mode = "dictation",
    InputLanguage = new List<string> { "en-IN", "hi" }, // English and Hindi
    OutputLanguage = "hi", // Output in Hindi
    OutputFormatTemplate = new()
    {
        new() { TemplateId = "clinical_notes_template" }
    }
};
```

### Example 3: EMR Integration

```csharp
var request = new TransactionInitRequest
{
    Mode = "consultation",
    ModelType = "pro",
    Speciality = "cardiology",
    OutputFormatTemplate = new()
    {
        new() { TemplateId = "eka_emr_template", CodificationNeeded = true }
    },
    AdditionalData = new Dictionary<string, object>
    {
        ["doctor"] = new { _id = "doc123", name = "Dr. Smith" },
        ["patient"] = new { _id = "pat456", name = "John Doe" },
        ["visitid"] = "visit789"
    }
};
```

## ⚙️ Configuration

### Supported Templates

| Template ID | Description |
|------------|-------------|
| `transcript_template` | Basic transcription |
| `clinical_notes_template` | Structured clinical notes |
| `eka_emr_template` | EMR-compatible format |

### Supported Languages

**Input/Output Languages:**
- `en-IN` - English (India)
- `en-US` - English (US)
- `hi` - Hindi
- `gu` - Gujarati
- `kn` - Kannada
- `ml` - Malayalam
- `ta` - Tamil
- `te` - Telugu
- `bn` - Bengali
- `mr` - Marathi
- `pa` - Punjabi
- `or` - Oriya

### Model Types

- `pro` - Most accurate (recommended)
- `lite` - Faster with lower latency

## 🔧 Troubleshooting

### Common Issues

**1. Authentication Fails**
```
Error: 401 Unauthorized
```
**Solution:** Verify your CLIENT_ID and CLIENT_SECRET are correct.

**2. File Upload Fails**
```
Error: Upload failed: 403 Forbidden
```
**Solution:** Ensure the presigned URL hasn't expired (valid for 15 minutes).

**3. Transcription Timeout**
```
Error: Polling timeout after 300 seconds
```
**Solution:** Increase `maxDurationSeconds` or check audio file size.

**4. .NET SDK Not Found**
```
Error: The command could not be loaded
```
**Solution:** Install .NET 8.0 SDK from https://dotnet.microsoft.com/download

### Debug Mode

Enable detailed logging:

```csharp
// In appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "EkaCare": "Trace"
    }
  }
}
```

### Testing API Endpoints

Use the included Swagger UI at `/swagger` or test with cURL:

```bash
# Health check
curl https://localhost:5001/api/transcription/status/test_123

# Complete workflow
curl -X POST https://localhost:5001/api/transcription/complete-workflow \
  -H "Content-Type: application/json" \
  -d @request.json
```

## 📝 License

This SDK is provided as-is for use with the EkaCare API.

## 🤝 Support

For issues and questions:
- **GitHub Issues**: [Create an issue](https://github.com/yourusername/ekacare-dotnet-sdk/issues)
- **EkaCare Docs**: https://docs.eka.care
- **Email**: support@eka.care

## 🎉 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

---

**Made with ❤️ for the EkaCare community**
