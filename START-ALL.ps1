Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Starting RAG Chatbot System" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Kill existing processes
Write-Host "Stopping existing processes..." -ForegroundColor Yellow
Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Start Backend
Write-Host "Starting Backend Server..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd C:\RAGChatbot\Backend\RAGChatbot.API; Write-Host 'Backend Server' -ForegroundColor Green; dotnet run --urls 'http://localhost:5000'"

Start-Sleep -Seconds 5

# Start Frontend  
Write-Host "Starting Frontend Server..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-ExecutionPolicy", "Bypass", "-Command", "& 'C:\RAGChatbot\start-frontend-server.ps1'"

Start-Sleep -Seconds 3

# Check status
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  System Status" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$backend = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue
$frontend = Get-NetTCPConnection -LocalPort 9000 -ErrorAction SilentlyContinue

if ($backend) {
    Write-Host "✓ Backend:  http://localhost:5000" -ForegroundColor Green
} else {
    Write-Host "✗ Backend:  NOT RUNNING" -ForegroundColor Red
}

if ($frontend) {
    Write-Host "✓ Frontend: http://localhost:9000" -ForegroundColor Green
} else {
    Write-Host "✗ Frontend: NOT RUNNING" -ForegroundColor Red
}

Write-Host ""
if ($backend -and $frontend) {
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ALL SYSTEMS READY!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Open in browser: http://localhost:9000" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Two PowerShell windows opened:" -ForegroundColor Yellow
    Write-Host "  1. Backend (shows API logs)" -ForegroundColor Gray
    Write-Host "  2. Frontend (shows HTTP server logs)" -ForegroundColor Gray
} else {
    Write-Host "Some services failed to start!" -ForegroundColor Red
}

Write-Host ""
