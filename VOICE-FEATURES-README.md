# RAG Chatbot - Voice-Enabled System

## ✅ COMPLETE - All Features Working!

### System Overview
- **Pure .NET Implementation** - No Python required
- **Single Server** - Frontend and backend on port 5000
- **Voice-Enabled** - Speech input and output support

---

## 🎤 Voice Features

### Voice Input (Speech Recognition)
- **Click** the 🎤 microphone button in the chat input
- **Speak** your question clearly
- **Watch** as your speech is converted to text automatically
- **Supported Browsers**: Chrome, Edge, Safari (with Web Speech API)

**Visual Indicators:**
- 🎤 = Ready to listen
- 🎙️ = Currently listening (with animated pulse effect)
- Button changes color to pink/red while recording

### Voice Output (Text-to-Speech)
- **Click** the 🔇 speaker button to enable voice output
- **Icon changes** to 🔊 when enabled
- **Bot responses** are automatically read aloud
- **Click again** to disable voice output

**Visual Indicators:**
- 🔇 = Voice output OFF
- 🔊 = Voice output ON
- Button pulses blue when speaking

---

## 📋 All Features

### ✓ Document Processing
- PDF files (with page tracking)
- CSV files
- Excel files (XLSX, XLS)
- Text files (TXT)
- Images (PNG, JPG, JPEG)
- YouTube video transcripts

### ✓ RAG Capabilities
- Text chunking (configurable size/overlap)
- Vector embeddings (1536 dimensions)
- Semantic search with cosine similarity
- Context-aware responses

### ✓ Chat Interface
- Clean, modern UI
- Real-time messaging
- Session management
- Document source references
- Exit command support

### ✓ Voice Control
- Speech-to-text input
- Text-to-speech output
- Visual feedback animations
- Toast notifications
- Browser microphone integration

---

## 🚀 How to Start

### Quick Start
```batch
C:\RAGChatbot\START.bat
```

### Manual Start
```powershell
cd C:\RAGChatbot\Backend\RAGChatbot.API
dotnet run --urls "http://localhost:5000"
```

---

## 🌐 Access Points

- **Main App**: http://localhost:5000
- **Swagger API**: http://localhost:5000/swagger

---

## 💡 Usage Guide

### 1. Upload Documents
1. Click **📄 Upload Document** tab
2. Enter a topic/category
3. Select your file
4. Click **Upload & Process**
5. Wait for confirmation

### 2. Chat with Voice Input
1. Go to **💬 Chat** tab
2. Click **🎤** microphone button
3. Allow microphone access (if first time)
4. Speak your question
5. Text appears automatically
6. Click **Send** or press Enter

### 3. Enable Voice Output
1. Click **🔇** speaker button
2. Icon changes to **🔊**
3. Send any message
4. Bot response is read aloud
5. Click **🔊** again to disable

### 4. Regular Text Chat
- Type your question in the text box
- Press Enter or click Send
- Bot responds with context from uploaded documents
- Type "exit" to end session

---

## 🔧 Technical Details

### Backend
- Framework: ASP.NET Core 8.0
- Language: C#
- Document Processing: iText7, CsvHelper, EPPlus
- Video: YoutubeExplode
- Vector Store: Custom in-memory FAISS-like implementation

### Frontend
- Pure HTML5, CSS3, JavaScript
- Web Speech API for voice recognition
- Speech Synthesis API for text-to-speech
- Responsive design
- Real-time DOM mutation observer for auto-speech

### Voice Technology
- **Speech Recognition**: Web Speech API (webkitSpeechRecognition)
- **Text-to-Speech**: SpeechSynthesis API
- **Language**: English (US) - configurable
- **Rate**: 1.0x speed
- **Cleanup**: Removes mock response prefixes for cleaner speech

---

## 📝 Notes

### Voice Feature Compatibility
- **Chrome**: Full support ✓
- **Edge**: Full support ✓
- **Safari**: Full support ✓
- **Firefox**: Limited support (TTS only)

### OpenAI Integration
- System works with **mock responses** by default
- To enable real AI: Add OpenAI API key to `appsettings.json`
- Mock responses include document context for testing

### Performance
- In-memory vector storage (fast, non-persistent)
- Supports thousands of document chunks
- Real-time response generation
- Minimal latency for voice processing

---

## 🎯 What's Been Accomplished

✅ Complete RAG chatbot system
✅ Multi-format document processing
✅ Vector search and retrieval
✅ Voice input (Speech Recognition)
✅ Voice output (Text-to-Speech)
✅ Pure .NET implementation (no Python)
✅ Single-server architecture
✅ Modern, responsive UI
✅ Real-time chat interface
✅ Mock mode for testing without API keys

---

## 🔄 System Status

**Server**: Running on port 5000
**Frontend**: Integrated with backend
**Voice**: Fully functional
**Documents**: Ready to upload
**Chat**: Ready to use

**All systems operational!** 🎉

---

*Last Updated: November 30, 2025*
