Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RAG Chatbot - Complete System Test" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check if ports are listening
Write-Host "TEST 1: Port Status" -ForegroundColor Yellow
$backend = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue
$frontend = Get-NetTCPConnection -LocalPort 8080 -ErrorAction SilentlyContinue

if ($backend) {
    Write-Host "  ✓ Backend port 5000: LISTENING" -ForegroundColor Green
} else {
    Write-Host "  ✗ Backend port 5000: NOT LISTENING" -ForegroundColor Red
}

if ($frontend) {
    Write-Host "  ✓ Frontend port 8080: LISTENING" -ForegroundColor Green
} else {
    Write-Host "  ✗ Frontend port 8080: NOT LISTENING" -ForegroundColor Red
}

Write-Host ""

# Test 2: HTTP Response
Write-Host "TEST 2: HTTP Response" -ForegroundColor Yellow
try {
    $backendTest = Invoke-WebRequest -Uri "http://localhost:5000/swagger/index.html" -TimeoutSec 3 -UseBasicParsing -ErrorAction Stop
    Write-Host "  ✓ Backend HTTP: Responding ($($backendTest.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Backend HTTP: Not responding" -ForegroundColor Red
}

try {
    $frontendTest = Invoke-WebRequest -Uri "http://localhost:8080" -TimeoutSec 3 -UseBasicParsing -ErrorAction Stop
    Write-Host "  ✓ Frontend HTTP: Responding ($($frontendTest.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Frontend HTTP: Not responding" -ForegroundColor Red
}

Write-Host ""

# Test 3: API Key Configuration
Write-Host "TEST 3: Configuration" -ForegroundColor Yellow
$settings = Get-Content "C:\RAGChatbot\Backend\RAGChatbot.API\appsettings.json" | ConvertFrom-Json
if ($settings.OpenAI.ApiKey -ne "your-openai-api-key-here") {
    Write-Host "  ✓ OpenAI API Key: Configured" -ForegroundColor Green
} else {
    Write-Host "  ✗ OpenAI API Key: NOT configured" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($backend -and $frontend) {
    Write-Host "READY TO USE!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Frontend URL: http://localhost:8080" -ForegroundColor Cyan
    Write-Host "Backend API: http://localhost:5000" -ForegroundColor Cyan
    Write-Host "API Docs: http://localhost:5000/swagger" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Open http://localhost:8080 in your browser!" -ForegroundColor Yellow
} else {
    Write-Host "SERVERS NOT RUNNING!" -ForegroundColor Red
    Write-Host ""
    Write-Host "To start servers, run:" -ForegroundColor Yellow
    Write-Host "  powershell -ExecutionPolicy Bypass -File C:\RAGChatbot\START.ps1" -ForegroundColor Cyan
}

Write-Host ""
