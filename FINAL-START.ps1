# Complete restart with logging

Write-Host "=" * 60 -ForegroundColor Cyan
Write-Host "COMPLETE SYSTEM RESTART WITH LOGGING" -ForegroundColor Green
Write-Host "=" * 60 -ForegroundColor Cyan

# Stop everything
Write-Host "`n[1/5] Stopping all processes..." -ForegroundColor Yellow
Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process python -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "*RAGChatbot*" } | Stop-Process -Force
Start-Sleep -Seconds 3

# Build backend
Write-Host "`n[2/5] Building backend..." -ForegroundColor Yellow
cd C:\RAGChatbot\Backend\RAGChatbot.API
$buildOutput = dotnet build --no-incremental 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build FAILED" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Build successful" -ForegroundColor Green

# Start backend with detailed logging
Write-Host "`n[3/5] Starting backend..." -ForegroundColor Yellow
$backendScript = @'
$Host.UI.RawUI.WindowTitle = "RAG BACKEND SERVER"
Write-Host "=" * 70 -ForegroundColor Green
Write-Host "BACKEND SERVER STARTING - WATCH FOR ERRORS BELOW" -ForegroundColor Green
Write-Host "=" * 70 -ForegroundColor Green
Write-Host ""
cd C:\RAGChatbot\Backend\RAGChatbot.API
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --urls "http://localhost:5000"
'@
$backendScript | Out-File -FilePath "C:\RAGChatbot\start-backend-temp.ps1" -Encoding UTF8
Start-Process powershell -ArgumentList "-NoExit", "-ExecutionPolicy", "Bypass", "-File", "C:\RAGChatbot\start-backend-temp.ps1"

Write-Host "Waiting for backend to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check if backend is running
Write-Host "`n[4/5] Checking backend status..." -ForegroundColor Yellow
$backendRunning = $false
for ($i = 1; $i -le 5; $i++) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/swagger" -TimeoutSec 3 -ErrorAction Stop
        Write-Host "✅ Backend is running on http://localhost:5000" -ForegroundColor Green
        $backendRunning = $true
        break
    }
    catch {
        Write-Host "Attempt $i/5 - Backend not ready yet..." -ForegroundColor Yellow
        Start-Sleep -Seconds 3
    }
}

if (-not $backendRunning) {
    Write-Host "❌ Backend failed to start. Check the Backend Server window for errors." -ForegroundColor Red
    exit 1
}

# Start frontend
Write-Host "`n[5/5] Starting frontend..." -ForegroundColor Yellow
$frontendScript = @'
$Host.UI.RawUI.WindowTitle = "RAG FRONTEND SERVER"
Write-Host "=" * 70 -ForegroundColor Cyan
Write-Host "FRONTEND SERVER" -ForegroundColor Cyan
Write-Host "=" * 70 -ForegroundColor Cyan
Write-Host ""
cd C:\RAGChatbot\Frontend-HTML
python -m http.server 9000
'@
$frontendScript | Out-File -FilePath "C:\RAGChatbot\start-frontend-temp.ps1" -Encoding UTF8
Start-Process powershell -ArgumentList "-NoExit", "-ExecutionPolicy", "Bypass", "-File", "C:\RAGChatbot\start-frontend-temp.ps1"

Write-Host ""
Write-Host "=" * 60 -ForegroundColor Green
Write-Host "SYSTEM STARTED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "=" * 60 -ForegroundColor Green
Write-Host ""
Write-Host "Backend:  http://localhost:5000" -ForegroundColor Cyan
Write-Host "Frontend: http://localhost:9000" -ForegroundColor Cyan
Write-Host "Swagger:  http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "IMPORTANT:" -ForegroundColor Yellow
Write-Host "- Watch the 'RAG BACKEND SERVER' window for any errors" -ForegroundColor Yellow
Write-Host "- If chat fails, the error details will appear there" -ForegroundColor Yellow
Write-Host ""
Write-Host "Testing system..." -ForegroundColor Yellow

# Simple test
try {
    Write-Host "`nTest 1: Sending chat message (no documents yet)..." -ForegroundColor Cyan
    $body = @{
        sessionId = "test-session"
        message = "hello"
        topK = 3
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/chat/message" `
        -Method POST `
        -Body $body `
        -ContentType "application/json" `
        -TimeoutSec 10
    
    Write-Host "✅ Chat working! Response:" -ForegroundColor Green
    Write-Host $response.response -ForegroundColor White
}
catch {
    Write-Host "❌ Chat test failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "CHECK THE 'RAG BACKEND SERVER' WINDOW FOR THE ACTUAL ERROR!" -ForegroundColor Yellow -BackgroundColor Red
    Write-Host ""
}

Write-Host "`nSystem is ready! Open http://localhost:9000 in your browser" -ForegroundColor Green
