# Start RAG Chatbot - HTML Version
# No Node.js required!

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RAG Chatbot - Starting HTML Version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if backend is running
$backendRunning = $false
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000" -Method HEAD -TimeoutSec 2 -ErrorAction SilentlyContinue
    $backendRunning = $true
} catch {
    $backendRunning = $false
}

if (-not $backendRunning) {
    Write-Host "⚠️  Backend is not running!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Please start the backend first:" -ForegroundColor Yellow
    Write-Host "  cd C:\RAGChatbot\Backend\RAGChatbot.API" -ForegroundColor White
    Write-Host "  dotnet run --urls `"http://localhost:5000`"" -ForegroundColor White
    Write-Host ""
    $startBackend = Read-Host "Would you like me to start it now? (y/n)"
    
    if ($startBackend -eq 'y' -or $startBackend -eq 'Y') {
        Write-Host ""
        Write-Host "Starting backend..." -ForegroundColor Green
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd C:\RAGChatbot\Backend\RAGChatbot.API; dotnet run --urls 'http://localhost:5000'"
        Write-Host "Waiting for backend to start..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5
    }
}

Write-Host ""
Write-Host "✅ Opening HTML Frontend..." -ForegroundColor Green
Write-Host ""

# Open the HTML file in default browser
$htmlPath = "C:\RAGChatbot\Frontend-HTML\index.html"
Start-Process $htmlPath

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Application Started!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Frontend: Opens in your browser" -ForegroundColor White
Write-Host "Backend:  http://localhost:5000" -ForegroundColor White
Write-Host "Swagger:  http://localhost:5000/swagger" -ForegroundColor White
Write-Host ""
Write-Host "📝 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Upload a document or video transcript" -ForegroundColor White
Write-Host "2. Start chatting with your data!" -ForegroundColor White
Write-Host "3. Type 'exit' to end the chat session" -ForegroundColor White
Write-Host ""
