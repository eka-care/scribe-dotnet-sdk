# EkaCare .NET SDK

A comprehensive .NET SDK for the EkaCare Medical Transcription API, supporting audio transcription with multiple templates and languages.

## 📋 Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Project Structure](#project-structure)
- [Running Locally](#running-locally)
  - [EkaCare.ConsoleExample](#method-1-ekacareconsolexample---command-line-application)
  - [EkaCare.WebApi](#method-2-ekacarewebapi---rest-api-service)
  - [Visual Studio](#method-3-visual-studio)
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

This SDK provides **three** ways to get started:

### 1. Using the SDK Directly in Your Code

Integrate the SDK directly into your .NET application:

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
        new() 
        { 
            TemplateId = "transcript_template",
            TemplateType = "custom", // Required: "custom", "default", etc.
            TemplateName = "Transcript Template" // Optional: descriptive name
        }
    }
};

await client.Transcription.InitializeTransactionAsync(presignedUrl.TxnId, request);

// Poll for results
var status = await client.Transcription.PollForCompletionAsync(presignedUrl.TxnId);
```

### 2. Using EkaCare.ConsoleExample

A ready-to-run console application that demonstrates the complete workflow:

```bash
cd EkaCare.ConsoleExample
# Update CLIENT_ID and CLIENT_SECRET in Program.cs
dotnet run
```

**Perfect for:**
- Learning how the SDK works
- Batch processing audio files
- Debugging and testing
- Command-line automation

### 3. Using EkaCare.WebApi

A REST API service with Swagger UI for web integration:

```bash
cd EkaCare.WebApi
# Update ClientId and ClientSecret in appsettings.json
dotnet run
# Open http://localhost:5000 for test page
# Open http://localhost:5000/swagger for API docs
```

**Perfect for:**
- Web application integration
- Mobile app backends
- Microservices architecture
- API-first development

### Which One Should I Use?

| Scenario | Recommended Approach |
|----------|---------------------|
| Learning the SDK | **EkaCare.ConsoleExample** |
| Building a .NET desktop app | **SDK directly** |
| Building a web/mobile backend | **EkaCare.WebApi** |
| Quick testing & debugging | **EkaCare.ConsoleExample** |
| REST API integration | **EkaCare.WebApi** |
| Batch processing scripts | **EkaCare.ConsoleExample** |
| Microservices | **EkaCare.WebApi** |

## 📁 Project Structure

This repository contains three main components:

### 1. EkaCare.SDK (Core Library)

The foundational SDK that provides all the functionality for interacting with the EkaCare API.

**Location:** `EkaCare.SDK/`

**Key Components:**
- `EkaCareClient.cs` - Main client for SDK initialization
- `AuthClient.cs` - OAuth2 authentication and token management
- `FileClient.cs` - File upload and presigned URL operations
- `TranscriptionClient.cs` - Transcription initialization, status checking, and polling
- `Models/` - Request and response models

**Usage:**
```csharp
// Reference in your .csproj
<ProjectReference Include="../EkaCare.SDK/EkaCare.SDK.csproj" />

// Use in your code
using EkaCare.SDK;
var client = new EkaCareClient(clientId, clientSecret);
```

### 2. EkaCare.ConsoleExample (Console Application)

A complete command-line application demonstrating the full transcription workflow.

**Location:** `EkaCare.ConsoleExample/`

**What It Contains:**
- `Program.cs` - Complete end-to-end transcription example
- Step-by-step execution with detailed console output
- Example configurations for different use cases
- Error handling and result decoding

**When to Use:**
- ✅ Learning how the SDK works
- ✅ Testing with your audio files
- ✅ Batch processing multiple files
- ✅ Creating automation scripts
- ✅ Debugging transcription issues
- ✅ Quick prototyping

**Key Features:**
- Detailed logging at each step
- Configurable templates and languages
- Automatic Base64 decoding
- JSON result formatting
- Token refresh example (commented)

### 3. EkaCare.WebApi (REST API Service)

A production-ready ASP.NET Core Web API exposing the SDK functionality through REST endpoints.

**Location:** `EkaCare.WebApi/`

**What It Contains:**
- `Program.cs` - API configuration and startup
- `Controllers/TranscriptionController.cs` - REST API endpoints
- `appsettings.json` - Configuration file
- `wwwroot/test-workflow.html` - Interactive test page
- Swagger UI for API documentation

**Available Endpoints:**
- Authentication (`/api/transcription/authenticate`)
- File operations (`/api/transcription/presigned-url`, `/upload-file`)
- Transcription (`/api/transcription/initialize`, `/status`, `/poll`)
- Complete workflow (`/api/transcription/complete-workflow`)

**When to Use:**
- ✅ Building web applications
- ✅ Mobile app backends (iOS, Android, React Native)
- ✅ Integrating with non-.NET systems
- ✅ Microservices architecture
- ✅ API-first development
- ✅ Multi-language client support

**Key Features:**
- RESTful API design
- Swagger/OpenAPI documentation
- CORS enabled for web integration
- Support for both configured and runtime credentials
- Automatic result decoding
- Interactive test page

### Project Dependencies

```
EkaCare.ConsoleExample
  └─ depends on → EkaCare.SDK

EkaCare.WebApi
  └─ depends on → EkaCare.SDK

EkaCare.SDK
  └─ standalone (no internal dependencies)
```

## 🏃 Running Locally

### Method 1: EkaCare.ConsoleExample - Command Line Application

The console application is a fully-featured example demonstrating the complete transcription workflow step-by-step. It's perfect for learning how the SDK works and for batch processing tasks.

#### Features:
- Step-by-step execution with detailed logging
- Authentication with token management
- File upload to S3
- Transcription initialization
- Automatic status polling
- Base64 result decoding and JSON formatting

#### Setup and Usage:

1. **Navigate to Console Example:**
   ```bash
   cd EkaCare.ConsoleExample
   ```

2. **Configure Credentials:**
   
   Edit `Program.cs` lines 12-18 and update the following constants:
   
   ```csharp
   private const string CLIENT_ID = "your_actual_client_id";
   private const string CLIENT_SECRET = "your_actual_client_secret";
   
   private static readonly List<string> AUDIO_FILE_PATHS = new()
   {
       @"C:\path\to\your\audio1.wav",
       @"/Users/username/audio2.mp3"  // Cross-platform paths supported
   };
   
   private const string TEMPLATE_ID = "transcript_template";
   // Available templates: "transcript_template", "eka_emr_template", "clinical_notes_template"
   ```

3. **Run the Application:**
   ```bash
   dotnet run
   ```

#### What It Does:

The console application performs these steps automatically:

1. **Authentication** - Logs in and obtains access token
2. **Get Presigned URL** - Retrieves S3 upload URL and transaction ID
3. **Upload Files** - Uploads audio files to S3 using presigned URL
4. **Initialize Transcription** - Starts the transcription job with your configuration
5. **Poll for Results** - Waits for completion and displays decoded results

#### Customization Options:

You can customize the transcription by modifying the `TransactionInitRequest` in lines 120-143:

```csharp
var request = new TransactionInitRequest
{
    Mode = "dictation",              // or "consultation"
    Transfer = "non-vaded",          // or "vaded"
    ModelType = "pro",               // or "lite"
    InputLanguage = new List<string> { "en-IN", "hi" }, // Multiple languages
    OutputLanguage = "en-IN",
    Speciality = "general_medicine", // or "cardiology", "orthopedics", etc.
    OutputFormatTemplate = new List<OutputFormatTemplate>
    {
        new OutputFormatTemplate
        {
            TemplateId = TEMPLATE_ID,
            TemplateType = "custom",     // Required: "custom", "default", etc.
            TemplateName = "My Custom Template", // Optional: descriptive name
            CodificationNeeded = false
        }
    },
    AdditionalData = new Dictionary<string, object>
    {
        ["patient"] = new { name = "John Doe", age = 35 },
        ["doctor"] = new { name = "Dr. Smith" }
    }
};
```

#### Expected Output:

```
=== EkaCare Transcription Example ===

=== Authentication ===
✓ Access Token: eyJhbGciOiJSUzI1NiIs...
✓ Refresh Token: eyJhbGciOiJIUzI1NiIs...
✓ Expires In: 300 seconds
✓ Authentication successful

=== Getting Presigned URL ===
✓ S3 URL: https://s3.amazonaws.com/...
✓ Transaction ID: txn_123456789

=== Uploading Audio Files ===
✓ Uploaded: audio1.wav
  Key: folder/audio1.wav
  Size: 1,234,567 bytes

=== Initializing Transcription ===
✓ Status: success
✓ Batch ID: batch_987654

=== Polling for Transcription Results ===
Polling... Status: in-progress
Polling... Status: in-progress
✓ Transcription completed

=== Transcription Results ===
Template: transcript_template
Status: success
Type: JSON

Decoded Result:
{
  "transcript": "Patient complains of headache and fever...",
  "duration": 45.3,
  "language": "en-IN"
}

=== Process Completed Successfully ===
```

---

### Method 2: EkaCare.WebApi - REST API Service

The Web API provides a RESTful interface for integrating EkaCare transcription into web applications, mobile apps, or other services. It includes Swagger UI for easy testing and a built-in test page.

#### Features:
- RESTful API endpoints for all SDK operations
- Swagger UI for API documentation and testing
- Interactive HTML test page
- CORS enabled for web integration
- Supports both configured credentials and runtime credentials
- Automatic Base64 decoding in responses

#### Setup and Usage:

1. **Navigate to Web API:**
   ```bash
   cd EkaCare.WebApi
   ```

2. **Configure Credentials:**
   
   Edit `appsettings.json`:
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*",
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
   
   The API will start on:
   - HTTP: `http://localhost:5000`
   - HTTPS: `https://localhost:5001`

4. **Access the Application:**
   
   - **Interactive Test Page**: `http://localhost:5000/` (redirects to test-workflow.html)
   - **Swagger UI**: `http://localhost:5000/swagger`
   - **API Endpoints**: `http://localhost:5000/api/transcription/...`

#### Available API Endpoints:

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/transcription/authenticate` | Authenticate and get access token |
| `POST` | `/api/transcription/refresh-token` | Refresh an expired access token |
| `POST` | `/api/transcription/presigned-url` | Get presigned URL for file upload |
| `POST` | `/api/transcription/upload` | Upload files using presigned URL |
| `POST` | `/api/transcription/upload-file` | Upload single file to S3 |
| `POST` | `/api/transcription/initialize/{txnId}` | Initialize transcription transaction |
| `POST` | `/api/transcription/status/{txnId}` | Get current transcription status |
| `GET`  | `/api/transcription/poll/{txnId}` | Poll until transcription completes |
| `POST` | `/api/transcription/complete-workflow` | Execute complete workflow in one call |

#### API Usage Examples:

**1. Complete Workflow (Recommended for Quick Start):**

This endpoint handles the entire process in one call:

```bash
curl -X POST http://localhost:5000/api/transcription/complete-workflow \
  -H "Content-Type: application/json" \
  -d '{
    "filePaths": ["/path/to/audio.wav"],
    "mode": "dictation",
    "modelType": "pro",
    "inputLanguage": ["en-IN"],
    "outputLanguage": "en-IN",
    "speciality": "general_medicine",
    "outputFormatTemplate": [
      {
        "templateId": "transcript_template",
        "templateType": "custom",
        "templateName": "Transcript Template",
        "codificationNeeded": false
      }
    ],
    "maxPollDurationSeconds": 300,
    "pollIntervalSeconds": 5
  }'
```

**Response:**
```json
{
  "message": "Workflow completed successfully",
  "txnId": "txn_123456789",
  "uploadResults": [
    {
      "fileName": "audio.wav",
      "key": "folder/audio.wav",
      "size": 1234567
    }
  ],
  "status": {
    "data": {
      "output": [
        {
          "templateId": "transcript_template",
          "status": "success",
          "type": "JSON",
          "decodedValue": {
            "transcript": "Patient complains of..."
          }
        }
      ]
    }
  }
}
```

**2. Step-by-Step Workflow:**

```bash
# Step 1: Authenticate
curl -X POST http://localhost:5000/api/transcription/authenticate

# Response: Save the access_token for subsequent requests
# {
#   "message": "Authentication successful",
#   "access_token": "eyJhbGci...",
#   "refresh_token": "eyJhbGci...",
#   "expires_in": 300
# }

# Step 2: Get Presigned URL
curl -X POST http://localhost:5000/api/transcription/presigned-url

# Response: Save txnId, uploadData.url, and folderPath
# {
#   "txnId": "txn_123456",
#   "folderPath": "/uploads/folder123/",
#   "uploadData": {
#     "url": "https://s3.amazonaws.com/bucket",
#     "fields": {...}
#   }
# }

# Step 3: Upload File
curl -X POST http://localhost:5000/api/transcription/upload-file \
  -H "Content-Type: application/json" \
  -d '{
    "filePath": "/path/to/audio.wav",
    "txnId": "txn_123456",
    "uploadUrl": "https://s3.amazonaws.com/bucket",
    "folderPath": "/uploads/folder123/",
    "fields": {}
  }'

# Step 4: Initialize Transcription
curl -X POST http://localhost:5000/api/transcription/initialize/txn_123456 \
  -H "Content-Type: application/json" \
  -d '{
    "request": {
      "mode": "dictation",
      "batchS3Url": "https://s3.amazonaws.com/bucket/uploads/folder123/",
      "clientGeneratedFiles": ["audio.wav"],
      "modelType": "pro",
      "inputLanguage": ["en-IN"],
      "outputLanguage": "en-IN",
      "outputFormatTemplate": [
        {
          "templateId": "transcript_template",
          "templateType": "custom",
          "templateName": "Transcript Template"
        }
      ]
    }
  }'

# Step 5: Poll for Completion
curl -X GET "http://localhost:5000/api/transcription/poll/txn_123456?maxDurationSeconds=300&pollIntervalSeconds=5"
```

**3. Using with Custom Credentials:**

You can pass credentials in the request body instead of using configured credentials:

```bash
curl -X POST http://localhost:5000/api/transcription/authenticate \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": "custom_client_id",
    "clientSecret": "custom_client_secret"
  }'
```

#### Testing with Swagger UI:

1. Open `http://localhost:5000/swagger`
2. Expand any endpoint (e.g., `/api/transcription/complete-workflow`)
3. Click "Try it out"
4. Fill in the request body with your data
5. Click "Execute"
6. View the response with decoded results

#### Integration Examples:

**JavaScript/TypeScript (Fetch API):**

```javascript
async function transcribeAudio(filePath) {
  const response = await fetch('http://localhost:5000/api/transcription/complete-workflow', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      filePaths: [filePath],
      mode: 'dictation',
      modelType: 'pro',
      outputFormatTemplate: [
        { 
          templateId: 'transcript_template',
          templateType: 'custom',
          templateName: 'Transcript Template'
        }
      ]
    })
  });
  
  const result = await response.json();
  console.log('Transcription:', result.status.data.output[0].decodedValue);
  return result;
}

// Usage
transcribeAudio('/path/to/audio.wav');
```

**Python (requests):**

```python
import requests

def transcribe_audio(file_path):
    url = 'http://localhost:5000/api/transcription/complete-workflow'
    payload = {
        'filePaths': [file_path],
        'mode': 'dictation',
        'modelType': 'pro',
        'outputFormatTemplate': [
            {
                'templateId': 'transcript_template',
                'templateType': 'custom',
                'templateName': 'Transcript Template'
            }
        ]
    }
    
    response = requests.post(url, json=payload)
    result = response.json()
    
    print('Transcription:', result['status']['data']['output'][0]['decodedValue'])
    return result

# Usage
transcribe_audio('/path/to/audio.wav')
```

**C# (HttpClient):**

```csharp
using System.Net.Http;
using System.Text.Json;

public async Task<string> TranscribeAudio(string filePath)
{
    using var client = new HttpClient();
    var request = new
    {
        filePaths = new[] { filePath },
        mode = "dictation",
        modelType = "pro",
        outputFormatTemplate = new[]
        {
            new { 
                templateId = "transcript_template",
                templateType = "custom",
                templateName = "Transcript Template"
            }
        }
    };
    
    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    
    var response = await client.PostAsync(
        "http://localhost:5000/api/transcription/complete-workflow",
        content);
    
    var result = await response.Content.ReadAsStringAsync();
    return result;
}
```

#### Configuration Options:

**Using Environment Variables:**

```bash
export EkaCare__ClientId="your_client_id"
export EkaCare__ClientSecret="your_client_secret"
dotnet run
```

**Using User Secrets (Development):**

```bash
dotnet user-secrets set "EkaCare:ClientId" "your_client_id"
dotnet user-secrets set "EkaCare:ClientSecret" "your_client_secret"
dotnet run
```

---

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
        new() 
        { 
            TemplateId = "transcript_template",
            TemplateType = "custom",
            TemplateName = "Transcript Template",
            CodificationNeeded = false 
        }
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
        new() 
        { 
            TemplateId = "transcript_template",
            TemplateType = "custom",
            TemplateName = "Transcript Template"
        }
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
        new() 
        { 
            TemplateId = "clinical_notes_template",
            TemplateType = "custom",
            TemplateName = "Clinical Notes Template"
        }
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
        new() 
        { 
            TemplateId = "eka_emr_template",
            TemplateType = "custom",
            TemplateName = "EkaCare EMR Template",
            CodificationNeeded = true 
        }
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

### Output Format Templates

Templates define how transcription results are structured and formatted. Each template requires three properties:

#### Template Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `template_id` | string | ✅ Yes | Unique identifier for the template |
| `template_type` | string | ✅ Yes | Type of template: `custom`, `default`, `emr`, etc. |
| `template_name` | string | ⚠️ Optional | Human-readable name for the template |
| `codification_needed` | boolean | ⚠️ Optional | Whether to include medical coding (default: false) |

#### Example Template Configuration

```csharp
OutputFormatTemplate = new List<OutputFormatTemplate>
{
    new OutputFormatTemplate
    {
        TemplateId = "ea016f6b-9bce-4d75-9f32-576ad20b4b19",
        TemplateType = "custom",  // Required
        TemplateName = "Live Gracious Template", // Optional but recommended
        CodificationNeeded = false
    }
}
```

```json
{
  "output_format_template": [
    {
      "template_id": "ea016f6b-9bce-4d75-9f32-576ad20b4b19",
      "template_type": "custom",
      "template_name": "Live Gracious Template"
    }
  ]
}
```

#### Common Template IDs

| Template ID | Template Type | Description |
|------------|---------------|-------------|
| `transcript_template` | `custom` | Basic transcription with timestamps |
| `clinical_notes_template` | `custom` | Structured clinical notes (SOAP format) |
| `eka_emr_template` | `custom` | EMR-compatible format with ICD codes |
| Your custom template ID | `custom` | Your organization's custom template |

#### Template Types

| Type | Description | Use Case |
|------|-------------|----------|
| `custom` | User-defined custom templates | Organization-specific formats, custom layouts |
| `default` | Standard EkaCare templates | Basic transcription, general use |
| `emr` | EMR integration templates | Electronic Medical Record systems |

**Note:** Always specify the `template_type` when configuring templates. The API requires this field to properly process your transcription request.

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

**5. Template Type Missing Error**
```
Error: 400 Bad Request - template_type is required
```
**Solution:** Ensure all `OutputFormatTemplate` objects include the `template_type` field:
```csharp
OutputFormatTemplate = new List<OutputFormatTemplate>
{
    new() 
    { 
        TemplateId = "your_template_id",
        TemplateType = "custom",  // Required!
        TemplateName = "Your Template Name"
    }
}
```

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

### EkaCare.ConsoleExample Issues

**5. Audio File Not Found**
```
Error: File not found: C:\path\to\audio.wav
```
**Solution:** 
- Verify the file path in the `AUDIO_FILE_PATHS` list
- Use absolute paths
- On Windows, use `@"C:\path\to\file.wav"` or `"C:\\path\\to\\file.wav"`
- On macOS/Linux, use `"/Users/username/audio.wav"`

**6. Configuration Not Updated**
```
Error: 401 Unauthorized with <client_id>
```
**Solution:** Make sure you replaced `<client_id>` and `<client_secret>` in `Program.cs` with your actual credentials.

**7. Compilation Errors**
```
Error: The type or namespace name 'EkaCare' could not be found
```
**Solution:** 
- Ensure you're in the correct directory: `cd EkaCare.ConsoleExample`
- Restore dependencies: `dotnet restore`
- Check that `EkaCare.SDK` project exists in the solution

### EkaCare.WebApi Issues

**8. Port Already in Use**
```
Error: Unable to bind to https://localhost:5001
```
**Solution:**
- Stop other applications using port 5000/5001
- Or specify a different port:
  ```bash
  dotnet run --urls "http://localhost:5500;https://localhost:5501"
  ```

**9. CORS Errors in Browser**
```
Error: Access to fetch blocked by CORS policy
```
**Solution:** The API already has CORS enabled. If still seeing errors:
- Check if you're using HTTPS when the API expects HTTP (or vice versa)
- Verify the request origin matches your configuration
- Check browser console for specific CORS error details

**10. Configuration Not Loaded**
```
Error: ClientId not configured
```
**Solution:**
- Verify `appsettings.json` has the correct structure
- Check that `EkaCare:ClientId` and `EkaCare:ClientSecret` are set
- Alternatively, use environment variables or user secrets

**11. Swagger Not Loading**
```
Error: 404 when accessing /swagger
```
**Solution:**
- Ensure you're running in Development mode
- Try accessing `/swagger/index.html` directly
- Check that the app started correctly: look for "Now listening on: http://localhost:5000"

**12. File Path Issues in API Requests**
```
Error: File not found when calling upload-file endpoint
```
**Solution:** 
- The API server must have access to the file path
- Use absolute paths
- Or consider uploading files as `multipart/form-data` and modifying the endpoint to accept file streams

## 📋 Quick Reference

### EkaCare.ConsoleExample Commands

```bash
# Navigate to project
cd EkaCare.ConsoleExample

# Edit configuration
# Update CLIENT_ID, CLIENT_SECRET, and AUDIO_FILE_PATHS in Program.cs

# Run the application
dotnet run

# Build for release
dotnet build -c Release

# Publish as self-contained executable
dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r osx-x64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained
```

### EkaCare.WebApi Commands

```bash
# Navigate to project
cd EkaCare.WebApi

# Edit configuration
# Update appsettings.json with your ClientId and ClientSecret

# Run the API
dotnet run

# Run with specific ports
dotnet run --urls "http://localhost:5500;https://localhost:5501"

# Run in production mode
dotnet run --environment Production

# Build for release
dotnet build -c Release

# Publish
dotnet publish -c Release -o ./publish
```

### Key URLs (when running locally)

| Service | URL | Description |
|---------|-----|-------------|
| **Console App** | N/A | Command-line interface |
| **Web API** | `http://localhost:5000` | Main API endpoint |
| **Web API (HTTPS)** | `https://localhost:5001` | Secure API endpoint |
| **Test Page** | `http://localhost:5000/` | Interactive test interface |
| **Swagger UI** | `http://localhost:5000/swagger` | API documentation |
| **API Base** | `http://localhost:5000/api/transcription` | REST endpoints |

### Common cURL Commands

```bash
# Complete workflow (easiest)
curl -X POST http://localhost:5000/api/transcription/complete-workflow \
  -H "Content-Type: application/json" \
  -d '{"filePaths":["/path/to/audio.wav"],"mode":"dictation","modelType":"pro","outputFormatTemplate":[{"templateId":"transcript_template","templateType":"custom","templateName":"Transcript Template"}]}'

# Authenticate only
curl -X POST http://localhost:5000/api/transcription/authenticate

# Get presigned URL
curl -X POST http://localhost:5000/api/transcription/presigned-url

# Check status
curl -X POST http://localhost:5000/api/transcription/status/txn_123456

# Poll for results
curl -X GET "http://localhost:5000/api/transcription/poll/txn_123456?maxDurationSeconds=300"
```

### Environment Variables

```bash
# Set credentials via environment variables (alternative to config files)

# Windows (PowerShell)
$env:EkaCare__ClientId="your_client_id"
$env:EkaCare__ClientSecret="your_client_secret"

# Windows (CMD)
set EkaCare__ClientId=your_client_id
set EkaCare__ClientSecret=your_client_secret

# macOS/Linux
export EkaCare__ClientId="your_client_id"
export EkaCare__ClientSecret="your_client_secret"
```

---

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
