# RAG Chatbot - Complete Setup & Troubleshooting Guide

## 🔧 Solutions Implemented

### ✅ Issue 1: Node.js Not Required - HTML Version Available!

**Problem Solved:** You don't need Node.js anymore!

**Solution:**
We've created a pure HTML/CSS/JavaScript version that runs directly in your browser.

**Two Frontend Options:**

**Option A: HTML Version (No Node.js needed) - RECOMMENDED**
```powershell
# Just open the file in your browser!
Start-Process "C:\RAGChatbot\Frontend-HTML\index.html"
```

**Option B: React Version (Requires Node.js)**
If you still want the React version:
1. Download Node.js from: https://nodejs.org/
2. Install and restart PowerShell
3. Run:
   ```powershell
   cd C:\RAGChatbot\Frontend
   npm install
   npm start
   ```

---

### Issue 2: Backend Port Already in Use ✅ FIXED

**Error Message:**
```
Failed to bind to address http://127.0.0.1:5000: address already in use
```

**Solution Applied:**
- Killed the existing process using port 5000
- The start script now automatically handles this

---

## 🚀 Quick Start - HTML Version (No Node.js!)

### Step 1: Start Backend

```powershell
cd C:\RAGChatbot\Backend\RAGChatbot.API
dotnet run --urls "http://localhost:5000"
```

### Step 2: Open Frontend

**Option A - Double Click:**
- Navigate to `C:\RAGChatbot\Frontend-HTML`
- Double-click `index.html`

**Option B - PowerShell:**
```powershell
Start-Process "C:\RAGChatbot\Frontend-HTML\index.html"
```

**Option C - Use Start Script:**
```powershell
cd C:\RAGChatbot
.\start-html.ps1
```

That's it! No Node.js, no npm install, no waiting!

---

## ⚙️ Configuration Required

### 1. Add OpenAI API Key

Edit: `C:\RAGChatbot\Backend\RAGChatbot.API\appsettings.json`

```json
{
  "OpenAI": {
    "ApiKey": "sk-proj-YOUR_ACTUAL_API_KEY_HERE",
    "EmbeddingModel": "text-embedding-3-small",
    "ChatModel": "gpt-4"
  }
}
```

**Get your API key:**
- Go to: https://platform.openai.com/api-keys
- Create new secret key
- Copy and paste into appsettings.json

---

## 🔍 Troubleshooting Commands

### Check if ports are in use:
```powershell
# Check port 5000 (Backend)
Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue

# Check port 3000 (Frontend)
Get-NetTCPConnection -LocalPort 3000 -ErrorAction SilentlyContinue
```

### Kill process on specific port:
```powershell
# Find process
$pid = (Get-NetTCPConnection -LocalPort 5000).OwningProcess
# Kill it
Stop-Process -Id $pid -Force
```

### Check if Node.js is installed:
```powershell
node --version
npm --version
```

### Check if .NET is installed:
```powershell
dotnet --version
```

### Clean and rebuild backend:
```powershell
cd C:\RAGChatbot\Backend\RAGChatbot.API
dotnet clean
dotnet restore
dotnet build
```

### Clean and reinstall frontend:
```powershell
cd C:\RAGChatbot\Frontend
Remove-Item -Recurse -Force node_modules
Remove-Item package-lock.json
npm install
```

---

## 📋 Installation Checklist - HTML Version

- [ ] Install .NET 8.0 SDK from microsoft.com/dotnet
- [ ] Get OpenAI API key from platform.openai.com
- [ ] Configure API key in appsettings.json
- [ ] Start backend: `cd C:\RAGChatbot\Backend\RAGChatbot.API` then `dotnet run --urls "http://localhost:5000"`
- [ ] Open HTML file: Double-click `C:\RAGChatbot\Frontend-HTML\index.html`
- [ ] Start chatting!

---

## 🌐 URLs After Starting

- **Frontend App:** file:///C:/RAGChatbot/Frontend-HTML/index.html (opens directly)
- **Backend API:** http://localhost:5000
- **API Documentation:** http://localhost:5000/swagger

**Note:** The HTML version opens as a local file, not on a web server. This is normal!

---

## ❓ Common Questions

**Q: Can I use a different port?**
```powershell
# Backend
dotnet run --urls "http://localhost:5001"

# Frontend (will prompt automatically if 3000 is taken)
```

**Q: Do I need both terminals running?**
Yes! The backend (API) and frontend (React app) run separately.

**Q: Can I use Gemini or other LLMs instead of OpenAI?**
Yes! Modify the `LLMService.cs` and `EmbeddingService.cs` files to use different APIs.

**Q: The app can't connect to the backend?**
- Check backend is running on port 5000
- Check CORS settings in Program.cs include your frontend URL
- Verify no firewall is blocking localhost connections

---

## 📞 Need More Help?

1. Check the main README.md for detailed documentation
2. Review Backend\README.md for backend-specific issues
3. Review Frontend\README.md for frontend-specific issues
4. Check the error logs in the terminal output

---

## 🎯 Next Steps After Setup

1. Upload a document (PDF, CSV, TXT)
2. Or paste a YouTube URL to extract transcript
3. Start chatting with your data!
4. Type 'exit' to end the chat session
