# Simple Start Script - No API Key Check
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "    Starting RAG Chatbot..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Clean up ports
Write-Host "`nCleaning up ports..." -ForegroundColor Yellow
$port5000 = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
if ($port5000) {
    $port5000 | ForEach-Object { Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue }
    Write-Host "  Killed process on port 5000" -ForegroundColor Gray
    Start-Sleep -Seconds 2
}

$port8080 = Get-NetTCPConnection -LocalPort 8080 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
if ($port8080) {
    $port8080 | ForEach-Object { Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue }
    Write-Host "  Killed process on port 8080" -ForegroundColor Gray
    Start-Sleep -Seconds 2
}

# Start Backend
Write-Host "`nStarting Backend Server..." -ForegroundColor Yellow
$backendPath = "C:\RAGChatbot\Backend\RAGChatbot.API"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$backendPath'; Write-Host 'Backend Server Starting...' -ForegroundColor Cyan; dotnet run --urls 'http://localhost:5000'"

Write-Host "  Waiting for backend..." -ForegroundColor Gray
Start-Sleep -Seconds 8

# Check backend
$backendConn = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue
if ($backendConn) {
    Write-Host "  Backend is RUNNING!" -ForegroundColor Green
} else {
    Write-Host "  Backend NOT detected - check the backend window" -ForegroundColor Red
}

# Start Frontend Server
Write-Host "`nStarting Frontend Server..." -ForegroundColor Yellow
$frontendServerPath = "C:\RAGChatbot\start-frontend-server.ps1"
Start-Process powershell -ArgumentList "-NoExit", "-ExecutionPolicy", "Bypass", "-File", $frontendServerPath

Write-Host "  Waiting for frontend..." -ForegroundColor Gray
Start-Sleep -Seconds 4

# Check frontend
$frontendConn = Get-NetTCPConnection -LocalPort 8080 -ErrorAction SilentlyContinue
if ($frontendConn) {
    Write-Host "  Frontend is RUNNING!" -ForegroundColor Green
} else {
    Write-Host "  Frontend NOT detected" -ForegroundColor Red
}

# Open Browser
Write-Host "`nOpening browser..." -ForegroundColor Yellow
Start-Sleep -Seconds 2
Start-Process "http://localhost:8080"

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "    STARTED!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "URLs:" -ForegroundColor White
Write-Host "  Frontend:  http://localhost:8080" -ForegroundColor Cyan
Write-Host "  Backend:   http://localhost:5000" -ForegroundColor Cyan
Write-Host "  Swagger:   http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "If portal not working:" -ForegroundColor Yellow
Write-Host "  1. Check both PowerShell windows for errors" -ForegroundColor White
Write-Host "  2. Make sure both servers show 'Now listening on: http://localhost'" -ForegroundColor White
Write-Host "  3. Add your OpenAI API key to appsettings.json" -ForegroundColor White
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
