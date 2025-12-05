Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RAG Chatbot - System Diagnostics" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check API Key
Write-Host "TEST 1: OpenAI API Key Configuration" -ForegroundColor Yellow
$settings = Get-Content "C:\RAGChatbot\Backend\RAGChatbot.API\appsettings.json" | ConvertFrom-Json
$apiKey = $settings.OpenAI.ApiKey

if ($apiKey -eq "your-openai-api-key-here") {
    Write-Host "  ✗ FAIL: API Key NOT configured" -ForegroundColor Red
    Write-Host "  Action: Run setup script or edit appsettings.json" -ForegroundColor Yellow
    $keyValid = $false
} elseif ($apiKey.StartsWith("sk-")) {
    Write-Host "  ✓ PASS: API Key configured and format looks valid" -ForegroundColor Green
    $keyValid = $true
} else {
    Write-Host "  ⚠ WARNING: API Key configured but format looks wrong" -ForegroundColor Yellow
    Write-Host "  Expected: Key starting with 'sk-'" -ForegroundColor Gray
    $keyValid = $false
}

Write-Host ""

# Test 2: Check Processes
Write-Host "TEST 2: Server Processes" -ForegroundColor Yellow
$backend = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue
$frontend = Get-NetTCPConnection -LocalPort 9000 -ErrorAction SilentlyContinue

if ($backend) {
    Write-Host "  ✓ Backend (5000): RUNNING" -ForegroundColor Green
    $backendRunning = $true
} else {
    Write-Host "  ✗ Backend (5000): NOT RUNNING" -ForegroundColor Red
    $backendRunning = $false
}

if ($frontend) {
    Write-Host "  ✓ Frontend (9000): RUNNING" -ForegroundColor Green
    $frontendRunning = $true
} else {
    Write-Host "  ✗ Frontend (9000): NOT RUNNING" -ForegroundColor Red
    $frontendRunning = $false
}

Write-Host ""

# Test 3: Test Backend API
if ($backendRunning) {
    Write-Host "TEST 3: Backend API Response" -ForegroundColor Yellow
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/swagger/index.html" -TimeoutSec 3 -UseBasicParsing -ErrorAction Stop
        Write-Host "  ✓ Backend responding to HTTP requests" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ Backend not responding properly" -ForegroundColor Red
    }
    Write-Host ""
}

# Summary and Actions
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SUMMARY & REQUIRED ACTIONS" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$issuesFound = $false

if (-not $keyValid) {
    $issuesFound = $true
    Write-Host "❌ ISSUE: OpenAI API Key" -ForegroundColor Red
    Write-Host "   Solution: Get API key from https://platform.openai.com/api-keys" -ForegroundColor White
    Write-Host "   Then run: powershell -ExecutionPolicy Bypass -File C:\RAGChatbot\SETUP-API-KEY.ps1" -ForegroundColor Cyan
    Write-Host ""
}

if (-not $backendRunning) {
    $issuesFound = $true
    Write-Host "❌ ISSUE: Backend Not Running" -ForegroundColor Red
    Write-Host "   Solution: Run the startup script" -ForegroundColor White
    Write-Host "   Command: powershell -ExecutionPolicy Bypass -File C:\RAGChatbot\START-ALL.ps1" -ForegroundColor Cyan
    Write-Host ""
}

if (-not $frontendRunning) {
    $issuesFound = $true
    Write-Host "❌ ISSUE: Frontend Not Running" -ForegroundColor Red
    Write-Host "   Solution: Run the startup script" -ForegroundColor White
    Write-Host "   Command: powershell -ExecutionPolicy Bypass -File C:\RAGChatbot\START-ALL.ps1" -ForegroundColor Cyan
    Write-Host ""
}

if (-not $issuesFound) {
    Write-Host "✅ ALL CHECKS PASSED!" -ForegroundColor Green
    Write-Host ""
    Write-Host "System URLs:" -ForegroundColor Yellow
    Write-Host "  Frontend: http://localhost:9000" -ForegroundColor Cyan
    Write-Host "  Backend:  http://localhost:5000" -ForegroundColor Cyan
    Write-Host "  API Docs: http://localhost:5000/swagger" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "If uploads still fail, the issue is likely:" -ForegroundColor Yellow
    Write-Host "  1. Invalid OpenAI API key" -ForegroundColor White
    Write-Host "  2. OpenAI account has no credits" -ForegroundColor White
    Write-Host "  3. API key doesn't have embedding permissions" -ForegroundColor White
} else {
    Write-Host "Please fix the issues above and try again." -ForegroundColor Yellow
}

Write-Host ""
