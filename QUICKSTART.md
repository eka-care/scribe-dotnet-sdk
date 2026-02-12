# Quick Start Guide

## 5-Minute Setup

### Step 1: Install .NET 8.0

```bash
# Check if already installed
dotnet --version

# If not installed, download from:
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### Step 2: Clone and Navigate

```bash
git clone <your-repo-url>
cd EkaCare.Solution
```

### Step 3: Choose Your Path

#### Option A: Console App (Simplest)

```bash
cd EkaCare.ConsoleExample

# Edit Program.cs with your credentials
# Update CLIENT_ID, CLIENT_SECRET, and AUDIO_FILE_PATHS

dotnet run
```

#### Option B: Web API (Recommended)

```bash
cd EkaCare.WebApi

# Edit appsettings.json
# Add your CLIENT_ID and CLIENT_SECRET

dotnet run

# Open browser to: https://localhost:5001/swagger
```

### Step 4: Test It

**Console App Output:**
```
=== Authentication ===
✓ Access Token: eyJhbGciOiJIUzI1NiIsI...
✓ Refresh Token: refresh_abc123...
✓ Authentication successful

=== Getting Presigned URL ===
✓ S3 URL: https://m-prod-ekascribe-batch.s3.amazonaws.com/
✓ Folder Path: EC_173210496011417/txn_301/20250617_105524/
✓ Transaction ID: txn_301

=== Uploading Audio Files ===
✓ Uploaded: audio.wav
  Key: EC_173210496011417/txn_301/20250617_105524/audio.wav
  Size: 2,847,392 bytes

=== Initializing Transcription ===
✓ Status: success
✓ Message: Transaction initialized successfully

=== Polling for Transcription Results ===
Polling status... (elapsed: 0.0s)
Waiting 5 seconds before next poll...
Polling status... (elapsed: 5.2s)
Transcription completed!

=== Transcription Results ===
Template: transcript_template
Status: success
Type: transcript

Decoded Result:
{
  "transcription": "Patient presents with fever and cough..."
}
```

**Web API Test (Swagger):**
1. Go to https://localhost:5001/swagger
2. Click "POST /api/transcription/authenticate"
3. Click "Try it out" → "Execute"
4. Copy the access token
5. Click "Authorize" at top, paste token
6. Try other endpoints!

## Common Workflows

### Workflow 1: Single File Transcription

```csharp
var client = new EkaCareClient("id", "secret");
var token = await client.Auth.LoginAsync();
client.SetAccessToken(token.AccessToken);

var presignedUrl = await client.Files.GetPresignedUrlAsync();
var uploads = await client.Files.UploadFilesAsync(
    presignedUrl, 
    new List<string> { "audio.wav" });

var request = new TransactionInitRequest
{
    Mode = "dictation",
    BatchS3Url = presignedUrl.UploadData.Url + presignedUrl.FolderPath,
    ClientGeneratedFiles = uploads.Select(u => u.FileName).ToList(),
    OutputFormatTemplate = new() { new() { TemplateId = "transcript_template" } }
};

await client.Transcription.InitializeTransactionAsync(presignedUrl.TxnId, request);
var result = await client.Transcription.PollForCompletionAsync(presignedUrl.TxnId);
```

### Workflow 2: Batch Processing

```csharp
var files = new List<string>
{
    "consultation1.wav",
    "consultation2.wav",
    "consultation3.wav"
};

var uploads = await client.Files.UploadFilesAsync(presignedUrl, files);
// ... rest same as above
```

### Workflow 3: Multi-language

```csharp
var request = new TransactionInitRequest
{
    InputLanguage = new List<string> { "en-IN", "hi" },
    OutputLanguage = "hi",
    OutputFormatTemplate = new()
    {
        new() { TemplateId = "clinical_notes_template" }
    }
};
```

## Troubleshooting One-Liners

```bash
# .NET not found?
winget install Microsoft.DotNet.SDK.8

# Port already in use?
dotnet run --urls "https://localhost:5002"

# Missing dependencies?
dotnet restore

# Clean build?
dotnet clean && dotnet build

# Run with verbose logging?
dotnet run --verbosity detailed
```

## Next Steps

- Read the full [README.md](README.md)
- Explore API documentation at `/swagger`
- Check out example requests in `Examples/` folder
- Join the EkaCare developer community

## Need Help?

- 📖 Docs: https://docs.eka.care
- 🐛 Issues: GitHub Issues tab
- 💬 Support: support@eka.care
