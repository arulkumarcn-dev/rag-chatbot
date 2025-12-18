# RAG Chatbot - Complete Startup Script
# Handles Tamil PDFs, Large Files (2GB), Enhanced Quiz with Hints

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RAG CHATBOT - TAMIL SUPPORT EDITION" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Features Enabled:" -ForegroundColor Yellow
Write-Host "  ✓ Tamil PDF Support" -ForegroundColor Green
Write-Host "  ✓ Large File Upload (up to 2GB)" -ForegroundColor Green
Write-Host "  ✓ English and Tamil Questions/Answers" -ForegroundColor Green
Write-Host "  ✓ Enhanced Quiz with Hints and Explanations" -ForegroundColor Green
Write-Host "  ✓ External References for Learning" -ForegroundColor Green
Write-Host ""

# Clean up old processes
Write-Host "[1/3] Cleaning up old processes..." -ForegroundColor Yellow
Get-Process -Name "dotnet","python" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
Write-Host "  ✓ Cleanup complete" -ForegroundColor Green

# Start Backend
Write-Host ""
Write-Host "[2/3] Starting Backend API..." -ForegroundColor Yellow
$backendPath = "$PSScriptRoot\Backend\RAGChatbot.API"
Start-Process powershell -ArgumentList "-NoExit","-Command","cd '$backendPath'; Write-Host 'Backend API Started' -ForegroundColor Green; Write-Host 'Port: 5000' -ForegroundColor Cyan; Write-Host 'Features: Tamil Support, 2GB Upload' -ForegroundColor Gray; dotnet run"

Write-Host "  Waiting for backend to initialize (8 seconds)..." -ForegroundColor Gray
Start-Sleep -Seconds 8

# Test backend
try {
    $null = Invoke-WebRequest -Uri "http://localhost:5000/swagger/index.html" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
    Write-Host "  ✓ Backend API is running on port 5000" -ForegroundColor Green
} catch {
    Write-Host "  ⚠ Backend may still be starting..." -ForegroundColor Yellow
}

# Start Frontend
Write-Host ""
Write-Host "[3/3] Starting Frontend Server..." -ForegroundColor Yellow

$pythonCmd = Get-Command python -ErrorAction SilentlyContinue
if (-not $pythonCmd) {
    $pythonCmd = Get-Command python3 -ErrorAction SilentlyContinue
}

if ($pythonCmd) {
    $frontendPath = "$PSScriptRoot\Frontend-HTML"
    Start-Process powershell -ArgumentList "-NoExit","-Command","cd '$frontendPath'; Write-Host 'Frontend Server Running' -ForegroundColor Green; Write-Host 'URL: http://localhost:8080' -ForegroundColor Yellow; python -m http.server 8080"
    
    Start-Sleep -Seconds 3
    Write-Host "  ✓ Frontend server started on port 8080" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  ✓ ALL SYSTEMS OPERATIONAL" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📍 URLs:" -ForegroundColor Cyan
    Write-Host "   Frontend:  http://localhost:8080" -ForegroundColor Yellow
    Write-Host "   Backend:   http://localhost:5000" -ForegroundColor Yellow
    Write-Host "   Swagger:   http://localhost:5000/swagger" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "🔐 Login Credentials:" -ForegroundColor Cyan
    Write-Host "   Username: admin" -ForegroundColor White
    Write-Host "   Password: admin123" -ForegroundColor White
    Write-Host ""
    Write-Host "📚 Features:" -ForegroundColor Cyan
    Write-Host "   • Upload Tamil PDFs (up to 2GB)" -ForegroundColor Gray
    Write-Host "   • Ask questions in Tamil or English" -ForegroundColor Gray
    Write-Host "   • Get exact answers from uploaded files" -ForegroundColor Gray
    Write-Host "   • Generate quizzes with hints and explanations" -ForegroundColor Gray
    Write-Host "   • External references for further study" -ForegroundColor Gray
    Write-Host ""
    
    Start-Sleep -Seconds 2
    Start-Process "http://localhost:8080"
    
} catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  ⚠ ERROR STARTING FRONTEND" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error: $_" -ForegroundColor Yellow
    Write-Host ""
} finally {
    if (-not $pythonFound) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Red
        Write-Host "  ⚠ PYTHON NOT FOUND" -ForegroundColor Yellow
        Write-Host "========================================" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please install Python:" -ForegroundColor Yellow
        Write-Host "  https://www.python.org/downloads/" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Or use Node.js:" -ForegroundColor Yellow
        Write-Host "  npx http-server Frontend-HTML -p 8080" -ForegroundColor Gray
        Write-Host ""
    }
}

Write-Host ""
Write-Host "Press Enter to exit (servers will keep running)..." -ForegroundColor Gray
Read-Host
