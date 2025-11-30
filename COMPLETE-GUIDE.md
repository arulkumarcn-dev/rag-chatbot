# 🚀 RAG Chatbot - Complete Step-by-Step Guide

## ⚡ Quick Summary

This RAG Chatbot lets you:
- Upload documents (PDF, CSV, TXT, images) or YouTube video transcripts
- Ask questions about your uploaded content
- Get AI-powered answers with source references
- Chat continuously until you type 'exit'

**Time to get started: 5-10 minutes**

---

## 📋 Prerequisites Check

Before starting, make sure you have:

### 1. Check .NET is Installed

```powershell
dotnet --version
```

**Expected Output:** `8.0.x` or higher

**If not installed:**
- Download from: https://dotnet.microsoft.com/download
- Choose ".NET 8.0 SDK"
- Install and restart PowerShell

### 2. Get OpenAI API Key

- Go to: https://platform.openai.com/api-keys
- Sign up or log in
- Click "Create new secret key"
- Copy the key (starts with `sk-proj-...`)
- Save it somewhere safe

**Cost:** OpenAI charges per API call. Typical usage: $0.01-$0.05 per conversation.

---

## 🔧 Step 1: Configure API Key

1. **Open the configuration file:**
   ```powershell
   notepad C:\RAGChatbot\Backend\RAGChatbot.API\appsettings.json
   ```

2. **Find this section:**
   ```json
   "OpenAI": {
     "ApiKey": "your-openai-api-key-here",
   ```

3. **Replace `your-openai-api-key-here` with your actual API key:**
   ```json
   "OpenAI": {
     "ApiKey": "sk-proj-ABC123XYZ...",
   ```

4. **Save and close** (Ctrl+S, then close Notepad)

---

## 🎯 Step 2: Start the Backend

Open PowerShell and run:

```powershell
cd C:\RAGChatbot\Backend\RAGChatbot.API
dotnet run --urls "http://localhost:5000"
```

**What to expect:**
```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**✅ Success!** Backend is running.

**❌ If you see an error:**

**Error: "Port already in use"**
```powershell
# Kill the process using port 5000
$pid = (Get-NetTCPConnection -LocalPort 5000).OwningProcess
Stop-Process -Id $pid -Force

# Try again
dotnet run --urls "http://localhost:5000"
```

**Error: "Unable to find package"**
```powershell
dotnet clean
dotnet restore
dotnet run --urls "http://localhost:5000"
```

**Keep this PowerShell window open!** The backend must run continuously.

---

## 🌐 Step 3: Open the Frontend

**Option A - PowerShell (Recommended):**

Open a **NEW PowerShell window** and run:
```powershell
cd C:\RAGChatbot
.\start-html.ps1
```

**Option B - Manual:**

1. Open File Explorer
2. Navigate to `C:\RAGChatbot\Frontend-HTML`
3. Double-click `index.html`

**What to expect:**
- Your default browser opens
- You see "RAG Chatbot" with a welcome message

**✅ Success!** Frontend is ready.

**❌ If you see errors in the chat:**
- Make sure backend is running (Step 2)
- Check browser console (Press F12, look for errors)
- Verify API key is configured correctly

---

## 📤 Step 4: Upload Your First Document

### Option A - Upload a PDF/CSV/TXT File

1. Click **"📄 Upload Document"** tab
2. Enter a topic: `Test Document`
3. Click **"Select File"**
4. Choose any PDF, CSV, or TXT file
5. Click **"Upload & Process"**

**What happens:**
- File is uploaded and processed
- Text is split into chunks
- Embeddings are generated
- Data is stored in vector database
- After 2 seconds, you're switched to Chat tab

**Expected result:**
```
Successfully processed filename.pdf
Created 15 chunks for indexing
```

### Option B - Process a YouTube Video

1. Click **"🎥 Upload Video"** tab
2. Enter a topic: `Tech Tutorial`
3. Paste a YouTube URL with captions, for example:
   ```
   https://www.youtube.com/watch?v=dQw4w9WgXcQ
   ```
4. Click **"Extract & Process"**

**What happens:**
- Transcript is extracted from video
- Text is chunked and embedded
- Ready for questions

**Note:** Video must have captions/subtitles available!

---

## 💬 Step 5: Start Chatting

1. Click **"💬 Chat"** tab (if not already there)

2. **Type a question** related to your uploaded document:
   ```
   What is this document about?
   ```

3. **Press Enter** or click **"Send"**

4. **Wait for response** (you'll see typing indicator)

5. **Review the answer:**
   - AI-generated response
   - Source references showing which chunks were used
   - Document name and chunk numbers

### Example Conversation:

```
You: What are the main topics covered?

Bot: Based on the uploaded document, the main topics are:
1. Introduction to RAG systems
2. Vector databases
3. LLM integration

📚 Sources:
Document.pdf - Chunk 1
"This document covers Retrieval Augmented Generation..."
```

### More Questions to Try:

```
Summarize the key points
What does it say about [specific topic]?
Find information about [keyword]
Compare sections 1 and 2
```

---

## 🛑 Step 6: Exit the Chat

When you're done chatting:

```
Type: exit
Press Enter
```

**Result:** Bot says goodbye. Refresh the page to start a new session.

---

## 🔄 Daily Usage Workflow

### Starting Up:

**Terminal 1:**
```powershell
cd C:\RAGChatbot\Backend\RAGChatbot.API
dotnet run --urls "http://localhost:5000"
```

**Terminal 2 or Browser:**
```powershell
Start-Process "C:\RAGChatbot\Frontend-HTML\index.html"
```

### Shutting Down:

1. Close browser tab
2. In backend terminal: Press `Ctrl+C`
3. Type `Y` to confirm

---

## 🧪 Testing the System

### Test 1: Simple Text File

1. Create a test file:
   ```powershell
   "Artificial Intelligence is transforming healthcare. Machine learning algorithms can detect diseases early." | Out-File -Encoding UTF8 C:\RAGChatbot\test.txt
   ```

2. Upload `test.txt` with topic: `AI Healthcare`

3. Ask: `What does the document say about AI in healthcare?`

4. **Expected:** Bot summarizes the content with source reference

### Test 2: Multiple Documents

1. Upload 2-3 different documents on different topics
2. Ask questions about each one
3. Bot should cite the correct source

### Test 3: YouTube Video

1. Find any YouTube video with captions (educational videos work best)
2. Upload the URL
3. Ask: `What is discussed in this video?`
4. **Expected:** Bot summarizes based on transcript

---

## ⚠️ Common Issues & Solutions

### Issue: "Failed to connect to backend"

**Symptoms:** Chat shows connection error

**Solution:**
```powershell
# Check backend is running
Get-NetTCPConnection -LocalPort 5000

# If not running, start it
cd C:\RAGChatbot\Backend\RAGChatbot.API
dotnet run --urls "http://localhost:5000"
```

### Issue: "Invalid API Key"

**Symptoms:** Error when uploading or chatting

**Solution:**
1. Check your OpenAI API key is valid
2. Make sure you have credits in your OpenAI account
3. Verify the key in `appsettings.json` is correct (no extra spaces)

### Issue: "No packages exist with this id"

**Symptoms:** `dotnet restore` fails

**Solution:**
```powershell
cd C:\RAGChatbot\Backend\RAGChatbot.API
dotnet clean
dotnet nuget locals all --clear
dotnet restore
```

### Issue: Video transcript fails

**Symptoms:** "No captions available"

**Solution:**
- Choose a different video that has captions
- Look for the "CC" button on YouTube - if it's there, it should work
- Try videos from major channels (TED, educational channels)

### Issue: PDF won't upload

**Symptoms:** Upload fails or shows error

**Solution:**
- Check PDF isn't password-protected
- Try a smaller PDF first (< 5MB)
- Make sure file isn't corrupted

### Issue: Slow responses

**Symptoms:** Takes a long time to get answers

**Causes:**
- Large documents create many chunks
- OpenAI API rate limits
- Network latency

**Solution:**
- Use smaller documents initially
- Wait a bit longer for first response
- Check your internet connection

---

## 📊 Understanding How It Works

### Behind the Scenes:

1. **Upload Document** →
   - Text is extracted (PDF parser, CSV reader, etc.)
   - Split into chunks (~1000 characters each with 200 char overlap)
   - Each chunk sent to OpenAI for embedding (creates vector)
   - Vectors stored in local database

2. **Ask Question** →
   - Your question is converted to embedding
   - System finds 5 most similar chunks (cosine similarity)
   - Chunks sent to GPT-4 as context
   - GPT-4 generates answer based on context
   - Answer returned with source references

### Tech Stack:

- **Backend:** .NET 8, C#, ASP.NET Core Web API
- **Frontend:** HTML, CSS, Vanilla JavaScript
- **Vector DB:** In-memory FAISS-like implementation
- **Embeddings:** OpenAI text-embedding-3-small
- **LLM:** OpenAI GPT-4
- **Document Processing:** iText7 (PDF), CsvHelper, YoutubeExplode

---

## 🎓 Advanced Usage

### Multiple Topics

Organize your knowledge base:
```
Topic: Healthcare → Upload medical documents
Topic: Finance → Upload financial reports  
Topic: Tech → Upload programming guides
```

### Chunk Customization

Edit `C:\RAGChatbot\Backend\RAGChatbot.API\Services\TextSplitter.cs`:

```csharp
// Change chunk size (default 1000) and overlap (default 200)
public List<string> SplitText(string text, int chunkSize = 2000, int overlap = 400)
```

### Change LLM Model

Edit `C:\RAGChatbot\Backend\RAGChatbot.API\appsettings.json`:

```json
"OpenAI": {
  "ChatModel": "gpt-3.5-turbo"  // Cheaper, faster, less accurate
  // or "gpt-4"                   // More expensive, slower, more accurate
}
```

### Use Different LLM (Gemini, Claude, etc.)

1. Edit `C:\RAGChatbot\Backend\RAGChatbot.API\Services\LLMService.cs`
2. Replace OpenAI API calls with your preferred LLM
3. Update configuration in `appsettings.json`

---

## 📈 Next Steps

### Beginner:
- ✅ Complete this guide
- ✅ Upload and chat with 3 different documents
- ✅ Try a YouTube video

### Intermediate:
- 📚 Organize documents by topic
- ⚙️ Adjust chunk sizes for better results
- 🎨 Customize the UI colors/layout

### Advanced:
- 🔧 Implement ChromaDB or Pinecone for persistent storage
- 🤖 Add support for Gemini or Claude
- 🌐 Deploy to Azure/AWS
- 📊 Add analytics and usage tracking

---

## 📞 Getting Help

### Documentation:
- Main README: `C:\RAGChatbot\README.md`
- Backend: `C:\RAGChatbot\Backend\README.md`
- Frontend: `C:\RAGChatbot\Frontend-HTML\README.md`

### Check Logs:
- Backend logs: In the PowerShell terminal running the backend
- Frontend logs: Browser console (F12 → Console tab)

### Common Commands:
```powershell
# Restart backend
cd C:\RAGChatbot\Backend\RAGChatbot.API
dotnet run --urls "http://localhost:5000"

# Clear vector database (start fresh)
Remove-Item C:\RAGChatbot\Backend\RAGChatbot.API\vectorstore -Recurse -Force

# View Swagger API docs
Start-Process "http://localhost:5000/swagger"
```

---

## ✅ Completion Checklist

Mark these off as you complete them:

- [ ] Installed .NET 8.0 SDK
- [ ] Got OpenAI API key
- [ ] Configured API key in appsettings.json
- [ ] Started backend successfully
- [ ] Opened frontend in browser
- [ ] Uploaded first document
- [ ] Asked first question
- [ ] Got AI response with sources
- [ ] Typed 'exit' to end session
- [ ] Understand how to restart the system

**Congratulations! You're now ready to use your RAG Chatbot! 🎉**

---

## 💡 Tips for Best Results

1. **Document Quality:**
   - Use well-formatted documents
   - Clear, readable text works best
   - PDFs with actual text (not scanned images)

2. **Question Phrasing:**
   - Be specific: "What are the symptoms?" vs "Tell me about it"
   - Reference document content: "According to the report..."
   - Ask follow-up questions for deeper insights

3. **Topic Organization:**
   - Use consistent topic names
   - Group related documents
   - Makes it easier to manage your knowledge base

4. **Performance:**
   - Smaller documents (< 10 pages) process faster
   - First query might be slower (warming up)
   - Subsequent queries are faster

---

**Ready to start? Follow the steps above! 🚀**
