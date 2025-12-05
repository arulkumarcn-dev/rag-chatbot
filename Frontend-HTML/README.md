# HTML Frontend - No Node.js Required!

This is a pure HTML/CSS/JavaScript version of the RAG Chatbot frontend. **No build tools, no npm, no dependencies!**

## 🚀 How to Use

### Quick Start

1. **Start the backend:**
   ```powershell
   cd C:\RAGChatbot\Backend\RAGChatbot.API
   dotnet run --urls "http://localhost:5000"
   ```

2. **Open the frontend:**
   - Simply double-click `index.html`
   - Or run: `Start-Process index.html`

That's it! The app will open in your default browser.

## 📁 Files

- `index.html` - Main HTML structure
- `styles.css` - All styling
- `app.js` - JavaScript functionality

## ✨ Features

All the same features as the React version:
- ✅ Chat interface with typing indicators
- ✅ Document upload (PDF, CSV, TXT, images)
- ✅ Video transcript processing (YouTube)
- ✅ Source citations
- ✅ Dynamic chat loop (until 'exit')
- ✅ Beautiful responsive design

## 🔧 Configuration

To change the backend API URL, edit `app.js`:

```javascript
const API_BASE_URL = 'http://localhost:5000/api/chat';
```

## 🌐 CORS Note

The HTML version uses `fetch()` to call the backend API. Make sure CORS is properly configured in the backend `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "null")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
```

Note: The `"null"` origin allows local file access.

## 📱 Mobile Responsive

The interface is fully responsive and works on:
- Desktop browsers
- Tablets
- Mobile phones

## 🎨 Customization

### Change Colors

Edit `styles.css` to change the gradient:

```css
background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
```

### Modify Layout

Edit `index.html` structure and adjust `styles.css` accordingly.

### Add Features

Edit `app.js` to add new functionality.

## ✅ Advantages Over React Version

1. **No build process** - Just open and run
2. **No dependencies** - No node_modules folder
3. **Instant startup** - No webpack compilation
4. **Easy to understand** - Plain JavaScript
5. **Lightweight** - Total size < 50KB

## ❓ Troubleshooting

**App doesn't connect to backend:**
- Verify backend is running on http://localhost:5000
- Check browser console (F12) for errors
- Ensure CORS includes "null" origin for local files

**Styling looks broken:**
- Make sure all three files are in the same directory
- Check browser console for CSS loading errors

**JavaScript not working:**
- Check browser console (F12) for errors
- Ensure app.js is loading correctly
- Try a different browser (Chrome, Edge, Firefox)

## 🚀 Deployment

To deploy this on a web server:

1. Upload all three files to your server
2. Update `API_BASE_URL` in `app.js` to your backend URL
3. Ensure backend CORS allows your domain
4. Access via your domain (e.g., https://yoursite.com)

## 📝 License

Same as the main project - MIT License
