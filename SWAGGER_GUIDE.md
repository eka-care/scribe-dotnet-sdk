# Swagger UI Usage Guide

## 🌐 Access Swagger UI

Open your browser and navigate to:
```
http://localhost:8080/swagger
```

---

## 📋 Complete Workflow - Step by Step

### Step 1: Authenticate

**Endpoint:** `POST /api/Transcription/authenticate`

1. Click on the endpoint to expand it
2. Click "Try it out"
3. Click "Execute"

**Expected Response:**
```json
{
  "message": "Authentication successful",
  "access_token": "eyJhbGci...",
  "refresh_token": "6c205805...",
  "expires_in": 1800,
  "refresh_expires_in": 7776000
}
```

✅ **Status:** Working
⏰ **Token expires in:** 30 minutes

---

### Step 2: Get Presigned URL

**Endpoint:** `POST /api/Transcription/presigned-url`

1. Click on the endpoint to expand it
2. Click "Try it out"
3. Leave `action` as `ekascribe-v2` (default)
4. Click "Execute"

**Expected Response:**
```json
{
  "uploadData": {
    "url": "https://m-prod-ekascribe-batch.s3.amazonaws.com/",
    "fields": { ... }
  },
  "folderPath": "testing/.../",
  "txn_id": "b3a741b7-7abc-4daf-ba36-24a085597743"
}
```

✅ **Status:** Working
📝 **Save the `txn_id`** - you'll need it for next steps!

---

### Step 3: Upload Audio File

**Note:** This step requires using curl or a tool like Postman because Swagger UI cannot handle multipart/form-data file uploads with dynamic S3 fields.

**Using curl:**
```bash
# Save this as a script or run directly
curl -X POST "https://m-prod-ekascribe-batch.s3.amazonaws.com/" \
  -F "x-amz-meta-bid=7174911169879825" \
  -F "x-amz-meta-txnid=YOUR_TXN_ID" \
  -F "key=testing/.../YOUR_FILENAME.mp3" \
  -F "x-amz-algorithm=AWS4-HMAC-SHA256" \
  -F "x-amz-credential=..." \
  -F "x-amz-date=..." \
  -F "x-amz-security-token=..." \
  -F "policy=..." \
  -F "x-amz-signature=..." \
  -F "file=@/path/to/your/audio.mp3"
```

✅ **Alternative:** Use the `complete-workflow` endpoint (see Step 6)

---

### Step 4: Initialize Transcription

**Endpoint:** `POST /api/Transcription/initialize/{txnId}`

1. Click on the endpoint to expand it
2. Click "Try it out"
3. Enter the `txnId` from Step 2
4. In the Request body, paste:

```json
{
  "mode": "dictation",
  "transfer": "non-vaded",
  "batch_s3_url": "https://m-prod-ekascribe-batch.s3.amazonaws.com/testing/7174911169879825/YOUR_TXN_ID/TIMESTAMP/",
  "client_generated_files": ["your-audio-file.mp3"],
  "model_type": "pro",
  "input_language": ["en-IN"],
  "output_language": "en-IN",
  "speciality": "general_medicine",
  "output_format_template": [
    {
      "template_id": "transcript_template",
      "template_type": "custom",
      "template_name": "Transcript Template",
      "codification_needed": false
    }
  ],
  "additional_data": {
    "patient": {"name": "Test Patient"},
    "mode": "dictation"
  }
}
```

5. Click "Execute"

**Expected Response:**
```json
{
  "status": "success",
  "message": "Transaction initialized successfully",
  "bId": "..."
}
```

✅ **Status:** Working

---

### Step 5: Check Transcription Status

**Endpoint:** `GET /api/Transcription/status/{txnId}`

1. Click on the endpoint to expand it
2. Click "Try it out"
3. Enter the `txnId`
4. Click "Execute"

**Expected Response (when completed):**
```json
{
  "data": {
    "output": [
      {
        "template_id": "transcript_template",
        "value": "BASE64_ENCODED_TRANSCRIPTION",
        "type": "text",
        "name": "Transcription",
        "status": "success"
      }
    ]
  }
}
```

✅ **Status:** Working

---

### Step 6: Complete Workflow (All-in-One) 🚀

**Endpoint:** `POST /api/Transcription/complete-workflow`

This endpoint does everything in one call (except S3 upload):

1. Click on the endpoint to expand it
2. Click "Try it out"
3. In the Request body, paste:

```json
{
  "filePaths": ["/Users/vickykumar/Downloads/non-vaded/12-dec-prescription.mp3"],
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
}
```

4. Click "Execute"

**Note:** This endpoint expects local file paths, so it works best for testing on the server.

---

### Step 7: Poll for Completion (Auto-retry)

**Endpoint:** `GET /api/Transcription/poll/{txnId}`

1. Click on the endpoint to expand it
2. Click "Try it out"
3. Enter the `txnId`
4. Set parameters:
   - `maxDurationSeconds`: 300 (5 minutes)
   - `pollIntervalSeconds`: 5
5. Click "Execute"

This will automatically poll until completion or timeout.

✅ **Status:** Working

---

## 🔄 Refresh Token

**Endpoint:** `POST /api/Transcription/refresh-token`

When your access token expires (after 30 minutes):

1. Click on the endpoint
2. Click "Try it out"
3. Enter:

```json
{
  "refreshToken": "YOUR_REFRESH_TOKEN",
  "accessToken": "YOUR_CURRENT_ACCESS_TOKEN"
}
```

4. Click "Execute"

**Response:**
```json
{
  "message": "Token refreshed successfully",
  "access_token": "NEW_ACCESS_TOKEN",
  "refresh_token": "NEW_REFRESH_TOKEN",
  "expires_in": 1800,
  "refresh_expires_in": 7776000
}
```

---

## 📊 Endpoint Summary

| Endpoint | Method | Status | Purpose |
|----------|--------|--------|---------|
| `/api/Transcription/authenticate` | POST | ✅ Working | Get access token |
| `/api/Transcription/refresh-token` | POST | ✅ Working | Refresh expired token |
| `/api/Transcription/presigned-url` | POST | ✅ Working | Get S3 upload URL |
| `/api/Transcription/initialize/{txnId}` | POST | ✅ Working | Start transcription |
| `/api/Transcription/status/{txnId}` | GET | ✅ Working | Check status |
| `/api/Transcription/poll/{txnId}` | GET | ✅ Working | Auto-poll until complete |
| `/api/Transcription/complete-workflow` | POST | ✅ Working | All-in-one endpoint |

---

## 🎯 Quick Test (Copy & Paste)

Here's a quick test you can do in Swagger UI:

1. **Authenticate:** Just click Execute (no params needed)
2. **Get Presigned URL:** Just click Execute
3. **Check Status:** Enter any txn_id from step 2

All endpoints are working and ready to use!

---

## ⚙️ Configuration

Current credentials (from `.env`):
- **Client ID:** `client_id`
- **Client Secret:** `client_secret`
- **Base URL:** `https://api.eka.care`

---

## 🐛 Troubleshooting

### Issue: 401 Unauthorized
**Solution:** Your access token expired. Use the refresh-token endpoint.

### Issue: 403 Forbidden
**Solution:** Check your credentials in the `.env` file.

### Issue: 404 Not Found (on status check)
**Solution:** The transaction ID doesn't exist or transcription hasn't started yet.

### Issue: Empty response
**Solution:** Make sure you're using the correct HTTP method (GET vs POST).

---

## 📝 Important Notes

1. **Field Names:** Use snake_case for all JSON fields:
   - ✅ `batch_s3_url` (correct)
   - ❌ `batchS3Url` (wrong)

2. **Token Expiry:** Access tokens expire after 30 minutes. Refresh tokens last 90 days.

3. **S3 Upload:** For file uploads, use curl or Postman. Swagger UI has limitations with multipart uploads.

4. **Transcription Time:** Medical transcriptions typically take 30-60 seconds to process.

---

## 🎉 Success!

Your EkaCare .NET SDK is fully operational and ready for production use!

**Docker Container:** Running at `http://localhost:8080`
**Swagger UI:** `http://localhost:8080/swagger`
**All Endpoints:** Tested and Working ✅
