Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  COMPLETE SYSTEM RESTART" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Kill everything
Write-Host "Step 1: Stopping all processes..." -ForegroundColor Yellow
Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3
Write-Host "  Done: Processes stopped" -ForegroundColor Green
Write-Host ""

# Step 2: Build
Write-Host "Step 2: Building backend..." -ForegroundColor Yellow
Push-Location "C:\RAGChatbot\Backend\RAGChatbot.API"
dotnet build | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "  Done: Build successful" -ForegroundColor Green
} else {
    Write-Host "  Error: Build failed!" -ForegroundColor Red
    Pop-Location
    Read-Host "Press Enter to exit"
    exit 1
}
Pop-Location
Write-Host ""

# Step 3: Start Backend
Write-Host "Step 3: Starting backend server..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", @"
`$Host.UI.RawUI.WindowTitle = 'Backend Server'
Write-Host ''
Write-Host '========================================'  -ForegroundColor Green
Write-Host '  BACKEND SERVER' -ForegroundColor Green
Write-Host '========================================'  -ForegroundColor Green
Write-Host ''
Set-Location 'C:\RAGChatbot\Backend\RAGChatbot.API'
dotnet run --urls 'http://localhost:5000'
"@
Start-Sleep -Seconds 8
Write-Host "  Done: Backend started" -ForegroundColor Green
Write-Host ""

# Step 4: Start Frontend
Write-Host "Step 4: Starting frontend server..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-ExecutionPolicy", "Bypass", "-Command", @"
`$Host.UI.RawUI.WindowTitle = 'Frontend Server'
& 'C:\RAGChatbot\start-frontend-server.ps1'
"@
Start-Sleep -Seconds 3
Write-Host "  Done: Frontend started" -ForegroundColor Green
Write-Host ""

# Step 5: Verify
Write-Host "Step 5: Verifying servers..." -ForegroundColor Yellow
Start-Sleep -Seconds 2
$backend = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue
$frontend = Get-NetTCPConnection -LocalPort 9000 -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SYSTEM STATUS" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($backend) {
    Write-Host "PASS Backend:  http://localhost:5000" -ForegroundColor Green
} else {
    Write-Host "FAIL Backend:  NOT RUNNING" -ForegroundColor Red
}

if ($frontend) {
    Write-Host "PASS Frontend: http://localhost:9000" -ForegroundColor Green
} else {
    Write-Host "FAIL Frontend: NOT RUNNING" -ForegroundColor Red
}

Write-Host ""

if ($backend -and $frontend) {
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ALL SYSTEMS READY!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Open: http://localhost:9000" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To see backend logs: Check 'Backend Server' window" -ForegroundColor Gray
    Write-Host "To see frontend logs: Check 'Frontend Server' window" -ForegroundColor Gray
    Write-Host ""
    Write-Host "If upload fails with '<' error:" -ForegroundColor Yellow
    Write-Host "  1. Check Backend Server window for actual error" -ForegroundColor White
    Write-Host "  2. Verify OpenAI API key is valid" -ForegroundColor White
    Write-Host "  3. Check OpenAI account has credits" -ForegroundColor White
} else {
    Write-Host "Some services failed to start!" -ForegroundColor Red
    Write-Host "Check the error messages above." -ForegroundColor Yellow
}

Write-Host ""
