# 🌐 How to Share Your RAG Chatbot

## ✅ Step 1: Make Sure Everything is Running

You should have TWO PowerShell windows open:

1. **Backend Server** - Running on port 5000
2. **Frontend Server** - Running on port 8080

## 📍 Current URLs (Local Only):

- **Frontend:** http://localhost:8080
- **Backend API:** http://localhost:5000
- **Swagger Docs:** http://localhost:5000/swagger

These URLs **ONLY work on your computer**.

---

## 🌍 Step 2: Get Public URLs with ngrok

### Option A: Quick Manual Setup

1. **Download ngrok:**
   - Go to: https://ngrok.com/download
   - Download for Windows
   - Extract the ZIP file

2. **Sign up (free):**
   - Go to: https://dashboard.ngrok.com/signup
   - Get your authtoken

3. **Setup ngrok:**
   ```powershell
   # Navigate to where you extracted ngrok
   cd C:\path\to\ngrok
   
   # Add your authtoken
   .\ngrok.exe config add-authtoken YOUR_TOKEN_HERE
   ```

4. **Create tunnels for BOTH servers:**

   **Terminal 1 - Backend tunnel:**
   ```powershell
   cd C:\path\to\ngrok
   .\ngrok.exe http 5000
   ```
   
   Copy the URL you get (e.g., `https://abc123.ngrok.io`)

   **Terminal 2 - Frontend tunnel:**
   ```powershell
   cd C:\path\to\ngrok
   .\ngrok.exe http 8080
   ```
   
   Copy this URL too (e.g., `https://xyz789.ngrok.io`)

5. **Update Frontend to use Backend ngrok URL:**
   
   Edit: `C:\RAGChatbot\Frontend-HTML\app.js`
   
   Change line 2 from:
   ```javascript
   const API_BASE_URL = 'http://localhost:5000/api/chat';
   ```
   
   To:
   ```javascript
   const API_BASE_URL = 'https://YOUR-BACKEND-NGROK-URL.ngrok.io/api/chat';
   ```

6. **Restart frontend server** (close and restart the PowerShell window)

7. **Share the frontend ngrok URL** with others!

---

### Option B: Automated PowerShell Script

```powershell
# Download ngrok automatically
$ngrokPath = "$env:USERPROFILE\Downloads\ngrok"
if (!(Test-Path $ngrokPath)) {
    Invoke-WebRequest -Uri "https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-windows-amd64.zip" -OutFile "$env:USERPROFILE\Downloads\ngrok.zip"
    Expand-Archive -Path "$env:USERPROFILE\Downloads\ngrok.zip" -DestinationPath $ngrokPath -Force
}

# Start backend tunnel
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd $ngrokPath; .\ngrok.exe http 5000 --log stdout"

# Wait a bit
Start-Sleep -Seconds 3

# Start frontend tunnel
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd $ngrokPath; .\ngrok.exe http 8080 --log stdout"

Write-Host "Ngrok tunnels started!" -ForegroundColor Green
Write-Host "Check the ngrok windows for your public URLs" -ForegroundColor Yellow
```

---

## 🎯 Current Architecture:

```
Your Computer:
├── Backend (localhost:5000)
│   └── ngrok tunnel → https://backend-xyz.ngrok.io
│
└── Frontend (localhost:8080)
    └── ngrok tunnel → https://frontend-abc.ngrok.io
```

**Share:** `https://frontend-abc.ngrok.io` with anyone!

---

## ⚠️ Important Notes:

1. **ngrok Free Tier Limits:**
   - URLs expire when you close ngrok
   - Limited to 40 connections/minute
   - New random URL each time you restart

2. **Keep All Windows Open:**
   - Backend server (port 5000)
   - Frontend server (port 8080)
   - Backend ngrok tunnel
   - Frontend ngrok tunnel

3. **API Key Security:**
   - Your OpenAI API key is in the backend
   - Anyone using your app will use YOUR API credits
   - Monitor usage at: https://platform.openai.com/usage

---

## 💰 For Permanent URLs:

### Deploy to Cloud (Recommended for Production):

**Azure (Microsoft):**
- Backend: Azure App Service (~$15/month)
- Frontend: Azure Static Web Apps (free tier available)
- Total: ~$15-30/month

**Netlify + Heroku:**
- Frontend: Netlify (free)
- Backend: Heroku (~$7/month)
- Total: ~$7/month

**AWS:**
- Frontend: S3 + CloudFront (~$1/month)
- Backend: Elastic Beanstalk (~$20/month)
- Total: ~$20-25/month

---

## 🧪 Quick Test:

After setting up ngrok:

1. Open the ngrok frontend URL in **Incognito/Private window**
2. Upload a test document
3. Ask questions
4. Share the URL with others to test

---

## 📞 Need Help?

Run this to check if everything is running:

```powershell
# Check backend
Test-NetConnection -ComputerName localhost -Port 5000

# Check frontend  
Test-NetConnection -ComputerName localhost -Port 8080
```

Both should show "TcpTestSucceeded : True"
