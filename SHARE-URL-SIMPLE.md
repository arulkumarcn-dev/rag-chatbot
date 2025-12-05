## 🌐 Your App is Running!

### ✅ Current Status:

| Service | Port | Local URL |
|---------|------|-----------|
| Backend API | 5000 | http://localhost:5000 |
| Frontend Web | 8080 | http://localhost:8080 |

### 📍 Access Right Now:

**On YOUR computer:** http://localhost:8080

**Important:** This URL only works on your machine!

---

## 🚀 To Share with Others - 3 Easy Steps:

### Step 1: Download ngrok

```powershell
# Run this in PowerShell:
Invoke-WebRequest -Uri "https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-windows-amd64.zip" -OutFile "$env:USERPROFILE\Downloads\ngrok.zip"
Expand-Archive -Path "$env:USERPROFILE\Downloads\ngrok.zip" -DestinationPath "$env:USERPROFILE\Downloads\ngrok" -Force
cd $env:USERPROFILE\Downloads\ngrok
```

### Step 2: Get Your Free ngrok Account

1. Go to: **https://dashboard.ngrok.com/signup**
2. Sign up (it's free!)
3. Copy your authtoken from: **https://dashboard.ngrok.com/get-started/your-authtoken**
4. Run this (replace YOUR_TOKEN):
   ```powershell
   .\ngrok.exe config add-authtoken YOUR_TOKEN_HERE
   ```

### Step 3: Create Public URLs

**Terminal 1 - Make Backend Public:**
```powershell
cd $env:USERPROFILE\Downloads\ngrok
.\ngrok.exe http 5000
```
You'll see: `Forwarding https://abc-123-xyz.ngrok.io -> http://localhost:5000`
**Copy this URL!** (the https one)

**Terminal 2 - Make Frontend Public:**
```powershell
cd $env:USERPROFILE\Downloads\ngrok
.\ngrok.exe http 8080
```
You'll see: `Forwarding https://def-456-xyz.ngrok.io -> http://localhost:8080`
**Copy this URL!**

---

## 🔧 Final Configuration:

1. **Edit Frontend Config:**
   ```powershell
   notepad C:\RAGChatbot\Frontend-HTML\app.js
   ```

2. **Change Line 2** from:
   ```javascript
   const API_BASE_URL = 'http://localhost:5000/api/chat';
   ```
   
   To (use YOUR backend ngrok URL):
   ```javascript
   const API_BASE_URL = 'https://YOUR-BACKEND-URL.ngrok.io/api/chat';
   ```

3. **Save the file**

4. **Restart frontend server** (close and reopen the PowerShell window running `start-frontend-server.ps1`)

---

## 🎉 Share Your App!

**Give this URL to anyone:**
```
https://YOUR-FRONTEND-URL.ngrok.io
```

They can now use your RAG Chatbot from anywhere!

---

## ⚠️ Important:

- **Keep all windows open** (backend, frontend, 2 ngrok tunnels)
- **ngrok free URLs** change each time you restart
- **Your OpenAI credits** will be used by anyone accessing the app
- **Monitor usage:** https://platform.openai.com/usage

---

## 🧪 Test It:

1. Open YOUR ngrok frontend URL in **Incognito/Private browser**
2. Upload a document
3. Chat with it
4. If it works, share the URL!

---

## 📊 What's Running:

```
Your Computer:
  ├── Backend (localhost:5000)
  │   └── ngrok → https://backend.ngrok.io (PUBLIC)
  │
  └── Frontend (localhost:8080)
      └── ngrok → https://frontend.ngrok.io (PUBLIC)
```

**Anyone with the frontend URL can access your chatbot! 🚀**
