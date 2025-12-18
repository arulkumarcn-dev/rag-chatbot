# RAG Chatbot - Tamil Support Startup Script
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RAG CHATBOT - TAMIL SUPPORT EDITION" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Features Enabled:" -ForegroundColor White
Write-Host "  Tamil PDF Support" -ForegroundColor Green
Write-Host "  Large File Upload (up to 2GB)" -ForegroundColor Green
Write-Host "  English and Tamil Questions/Answers" -ForegroundColor Green
Write-Host "  Enhanced Quiz with Hints and Explanations" -ForegroundColor Green
Write-Host "  External References for Learning" -ForegroundColor Green
Write-Host ""

# Clean up old processes
Write-Host "[1/3] Cleaning up old processes..." -ForegroundColor Yellow
Get-Process -Name dotnet,python -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
Write-Host "  Done" -ForegroundColor Green
Write-Host ""

# Start Backend
Write-Host "[2/3] Starting Backend API..." -ForegroundColor Yellow
Set-Location "Backend\RAGChatbot.API"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run"
Set-Location "..\..\"
Write-Host "  Backend starting on http://localhost:5000" -ForegroundColor Green
Write-Host "  Waiting for initialization (8 seconds)..." -ForegroundColor Gray
Start-Sleep -Seconds 8
Write-Host ""

# Start Frontend
Write-Host "[3/3] Starting Frontend Server..." -ForegroundColor Yellow
$pythonCmd = Get-Command python -ErrorAction SilentlyContinue
if ($pythonCmd) {
    Set-Location "Frontend-HTML"
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "python -m http.server 8080"
    Set-Location ".."
    Write-Host "  Frontend starting on http://localhost:8080" -ForegroundColor Green
    Start-Sleep -Seconds 2
    Write-Host ""
    
    # Success banner
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  SERVERS ARE RUNNING!" -ForegroundColor White
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Access URLs:" -ForegroundColor White
    Write-Host "  Frontend: http://localhost:8080" -ForegroundColor Cyan
    Write-Host "  Backend:  http://localhost:5000" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Login Credentials:" -ForegroundColor White
    Write-Host "  Username: admin" -ForegroundColor Gray
    Write-Host "  Password: admin123" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Tamil Support Features:" -ForegroundColor White
    Write-Host "  Upload Tamil PDFs, CSVs, TXT, DOCX, or Videos" -ForegroundColor Gray
    Write-Host "  Ask questions in Tamil or English" -ForegroundColor Gray
    Write-Host "  Get exact answers from uploaded content" -ForegroundColor Gray
    Write-Host "  Generate quizzes with hints and explanations" -ForegroundColor Gray
    Write-Host "  External references for further study" -ForegroundColor Gray
    Write-Host ""
    
    Start-Sleep -Seconds 2
    Start-Process "http://localhost:8080"
    
} else {
    Write-Host "  Python not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install Python:" -ForegroundColor Yellow
    Write-Host "  https://www.python.org/downloads/" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Or use Node.js:" -ForegroundColor Yellow
    Write-Host "  npx http-server Frontend-HTML -p 8080" -ForegroundColor Gray
    Write-Host ""
}

Write-Host ""
Write-Host "Press Enter to exit (servers will keep running)..." -ForegroundColor Gray
Read-Host
