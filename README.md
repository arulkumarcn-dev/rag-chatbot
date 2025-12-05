# RAG Chatbot - AI-Powered Document Assistant

A full-stack RAG (Retrieval Augmented Generation) chatbot that allows you to upload documents, videos, and other content, then ask questions about them using AI.

## 🌟 Features

### 💬 Core RAG Features
- **Multi-Format Support**: PDF, CSV, TXT, Excel, and YouTube video transcripts
- **Intelligent Text Chunking**: Automatically splits large documents with overlap
- **Vector Search**: FAISS-based vector database for semantic search
- **LLM Integration**: OpenAI GPT-4 and embeddings support
- **Source Citations**: Responses include references to source documents and chunks
- **Dynamic Chat Loop**: Chat continues until user types 'exit'
- **Modern UI**: Beautiful React frontend with responsive design

### 🌐 Multilingual Voice Support (NEW!)
- **9 Languages**: English, Tamil, Hindi, Telugu, Malayalam, Kannada, Bengali, Gujarati, Punjabi
- **Voice Input**: Speech-to-text recognition for all supported languages
- **Voice Output**: Text-to-speech synthesis with automatic language detection
- **UTF-8 Processing**: Full support for multilingual PDFs and documents
- **Language Auto-Detection**: Automatically identifies and processes document language

### 📝 Quiz Generation System (NEW!)
- **AI-Powered Quiz Creation**: Generate quizzes from uploaded documents
- **Multiple Choice Format**: 4 options per question with single correct answer
- **Instant Feedback**: Real-time answer validation with visual indicators
- **Enhanced Explanations**: Comprehensive explanations with external knowledge beyond document content
- **Results Tracking**: Save quiz history with statistics and analytics
- **Performance Dashboard**: View total quizzes, correct/wrong answers, and average scores
- **Date Filtering**: Filter results by All/Today/Week/Month
- **Export & Import**: Download quiz results as JSON for backup

### 🎯 External Knowledge Integration (NEW!)
- **Comprehensive Explanations**: Quiz answers include external knowledge, not just document content
- **Educational Enhancement**: Related concepts, practical applications, and broader context
- **Multi-Source Learning**: Combines document facts with general AI knowledge base

## 🏗️ Architecture

### Backend (.NET 8)
- **API Layer**: RESTful API with Swagger documentation
- **Document Processing**: Extracts text from PDF, CSV, images, and videos
- **Text Splitting**: Intelligent chunking with configurable overlap
- **Vector Store**: FAISS for efficient similarity search
- **Embedding Service**: OpenAI text-embedding-3-small
- **LLM Service**: OpenAI GPT-4 for response generation

### Frontend (React)
- **Chat Interface**: Real-time messaging with typing indicators
- **Document Upload**: Drag-and-drop file upload
- **Video Processing**: YouTube transcript extraction
- **Source Display**: Shows relevant chunks used for responses

## 📋 Prerequisites

- .NET 8.0 SDK
- Node.js 18+ and npm
- OpenAI API Key (or Gemini/Grok API key)

## 🚀 Setup Instructions

### Backend Setup

1. **Navigate to backend directory**:
```powershell
cd C:\RAGChatbot\Backend\RAGChatbot.API
```

2. **Configure API Keys**:
Edit `appsettings.json` and add your API keys:
```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key-here",
    "EmbeddingModel": "text-embedding-3-small",
    "ChatModel": "gpt-4"
  }
}
```

3. **Restore NuGet packages**:
```powershell
dotnet restore
```

4. **Run the backend**:
```powershell
dotnet run --urls "http://localhost:5000"
```

The API will be available at `http://localhost:5000`
Swagger UI: `http://localhost:5000/swagger`

### Frontend Setup

1. **Navigate to frontend directory**:
```powershell
cd C:\RAGChatbot\Frontend
```

2. **Install dependencies**:
```powershell
npm install
```

3. **Start the development server**:
```powershell
npm start
```

The app will open at `http://localhost:3000`

## 📖 Usage Guide

### 1. Upload Documents

**Option A - File Upload**:
1. Click "Upload Document" tab
2. Enter a topic (e.g., "Healthcare", "Data Engineering")
3. Select a file (PDF, CSV, TXT, PNG, JPG)
4. Click "Upload & Process"

**Option B - Video Transcript**:
1. Click "Upload Video" tab
2. Enter a topic
3. Paste YouTube URL
4. Click "Extract & Process"

### 2. Chat with Your Data

1. Click "Chat" tab
2. Type your question
3. The bot will:
   - Search for relevant document chunks
   - Generate a response using GPT-4
   - Show source references
4. Type `exit` to end the session

### 3. Example Questions

After uploading healthcare documents:
- "What are the main symptoms discussed?"
- "Summarize the treatment protocols"
- "What does the document say about prevention?"

## 🔧 Configuration Options

### Chunk Size and Overlap
Edit `TextSplitter.cs`:
```csharp
SplitText(text, chunkSize: 1000, overlap: 200)
```

### Number of Retrieved Documents
In chat requests, adjust `topK`:
```javascript
topK: 5  // Number of chunks to retrieve
```

### Change LLM Provider

To use Gemini instead of OpenAI, modify `LLMService.cs`:
```csharp
// Update API endpoint and request format for Gemini
```

## 📁 Project Structure

```
RAGChatbot/
├── Backend/
│   └── RAGChatbot.API/
│       ├── Controllers/
│       │   └── ChatController.cs
│       ├── Models/
│       │   ├── ChatRequest.cs
│       │   └── DocumentUpload.cs
│       ├── Services/
│       │   ├── DocumentProcessor.cs
│       │   ├── TextSplitter.cs
│       │   ├── FAISSVectorStore.cs
│       │   ├── EmbeddingService.cs
│       │   ├── LLMService.cs
│       │   └── ChatService.cs
│       ├── Program.cs
│       └── appsettings.json
│
└── Frontend/
    ├── public/
    │   └── index.html
    ├── src/
    │   ├── components/
    │   │   ├── ChatInterface.js
    │   │   ├── DocumentUpload.js
    │   │   └── VideoUpload.js
    │   ├── App.js
    │   └── index.js
    └── package.json
```

## 🔌 API Endpoints

### POST `/api/chat/message`
Send a chat message and get AI response
```json
{
  "sessionId": "uuid",
  "message": "Your question",
  "topK": 5
}
```

### POST `/api/chat/upload`
Upload and process a document
- Form data with `file` and `topic`

### POST `/api/chat/upload-video`
Process YouTube video transcript
```json
{
  "videoUrl": "https://youtube.com/...",
  "topic": "Topic name"
}
```

## 🎨 Customization

### Change Theme Colors
Edit `App.css` gradient:
```css
background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
```

### Modify Chat Behavior
Edit system message in `LLMService.cs`:
```csharp
var systemMessage = "Your custom instructions...";
```

## 📚 Documentation

### Testing & Verification
- **[COMPLETE-FUNCTIONALITY-TEST.md](COMPLETE-FUNCTIONALITY-TEST.md)** - Comprehensive test scenarios with 20 detailed mock tests
- **[QUICK-TEST-CHECKLIST.md](QUICK-TEST-CHECKLIST.md)** - Quick checklist for user verification
- **[IMPLEMENTATION-CHECKLIST.md](IMPLEMENTATION-CHECKLIST.md)** - Full feature implementation status

### Feature Guides
- **[MULTILINGUAL-VOICE-GUIDE.md](MULTILINGUAL-VOICE-GUIDE.md)** - Complete guide for 9-language voice support
- **[MULTILINGUAL-QUICK-START.md](MULTILINGUAL-QUICK-START.md)** - Quick reference for multilingual features
- **[QUIZ-FEATURE-GUIDE.md](QUIZ-FEATURE-GUIDE.md)** - Complete quiz system documentation
- **[QUIZ-QUICK-START.md](QUIZ-QUICK-START.md)** - Quick start for quiz generation

### Setup & Deployment
- **[SETUP-GUIDE.md](SETUP-GUIDE.md)** - Detailed setup instructions
- **[AZURE-DEPLOYMENT-GUIDE.md](AZURE-DEPLOYMENT-GUIDE.md)** - Azure deployment guide
- **[HOW-TO-RUN.md](HOW-TO-RUN.md)** - Running the application
- **[QUICK-FIX.ps1](QUICK-FIX.ps1)** - Quick troubleshooting script

## 🐛 Troubleshooting

**Backend won't start**:
- Ensure .NET 8.0 SDK is installed: `dotnet --version`
- Check port 5000 is not in use
- Verify OpenAI API key in `appsettings.json`

**Frontend connection error**:
- Verify backend is running on port 5000
- Check CORS settings in `Program.cs`

**Document upload fails**:
- Check file format is supported
- Verify OpenAI API key is valid
- Check API rate limits

**Voice features not working**:
- Use Chrome/Edge for best support
- Install language packs (Windows/Mac settings)
- Grant microphone permissions
- See [MULTILINGUAL-VOICE-GUIDE.md](MULTILINGUAL-VOICE-GUIDE.md)

**Quiz generation fails**:
- Ensure documents uploaded first
- Check OpenAI API quota
- Verify internet connection
- See [QUIZ-FEATURE-GUIDE.md](QUIZ-FEATURE-GUIDE.md)

**Video transcript error**:
- Ensure video has captions enabled
- YouTube URL must be valid
- Check internet connection

## 📝 Supported Document Types

| Type | Extensions | Notes |
|------|-----------|-------|
| PDF | .pdf | Page numbers preserved |
| CSV | .csv | Converted to text format |
| Text | .txt | Direct processing |
| Images | .png, .jpg, .jpeg | Requires OCR setup |
| Videos | YouTube URLs | Captions must be available |

## 🚀 Future Enhancements

- [ ] Add ChromaDB as alternative vector store
- [ ] Implement Gemini and Grok LLM options
- [ ] Add document deletion functionality
- [ ] Implement conversation history persistence
- [ ] Add multi-language support
- [ ] Implement OCR for images
- [ ] Add audio file transcription
- [ ] Export chat conversations

## 📄 License

This project is open source and available under the MIT License.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📧 Support

For issues or questions, please create an issue in the repository.

---

**Built with ❤️ using .NET 8 and React**
