# RAG Chatbot - Complete System Test & Launch
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RAG CHATBOT - COMPLETE SYSTEM TEST" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Clean processes
Write-Host "[1/5] Cleaning old processes..." -ForegroundColor Yellow
Get-Process -Name dotnet,python -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
Write-Host "  ✓ Cleanup complete" -ForegroundColor Green
Write-Host ""

# Step 2: Start Backend
Write-Host "[2/5] Starting Backend API..." -ForegroundColor Yellow
Set-Location "Backend\RAGChatbot.API"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Write-Host 'RAG CHATBOT BACKEND API' -ForegroundColor Cyan; Write-Host '========================' -ForegroundColor Cyan; Write-Host ''; dotnet run"
Set-Location "..\.."
Write-Host "  ✓ Backend starting on http://localhost:5000" -ForegroundColor Green
Write-Host "  ⏳ Waiting 10 seconds for initialization..." -ForegroundColor Gray
Start-Sleep -Seconds 10
Write-Host ""

# Step 3: Start Frontend  
Write-Host "[3/5] Starting Frontend Server..." -ForegroundColor Yellow
Set-Location "Frontend-HTML"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Write-Host 'RAG CHATBOT FRONTEND' -ForegroundColor Cyan; Write-Host '====================' -ForegroundColor Cyan; Write-Host ''; python -m http.server 8080"
Set-Location ".."
Write-Host "  ✓ Frontend starting on http://localhost:8080" -ForegroundColor Green
Write-Host "  ⏳ Waiting 3 seconds..." -ForegroundColor Gray
Start-Sleep -Seconds 3
Write-Host ""

# Step 4: Verify servers
Write-Host "[4/5] Verifying Servers..." -ForegroundColor Yellow
$backendRunning = Test-NetConnection -ComputerName localhost -Port 5000 -InformationLevel Quiet -WarningAction SilentlyContinue
$frontendRunning = Test-NetConnection -ComputerName localhost -Port 8080 -InformationLevel Quiet -WarningAction SilentlyContinue

Write-Host "  Backend API (Port 5000): " -NoNewline
if ($backendRunning) {
    Write-Host "RUNNING" -ForegroundColor Green
} else {
    Write-Host "NOT RUNNING" -ForegroundColor Red
}

Write-Host "  Frontend Server (Port 8080): " -NoNewline
if ($frontendRunning) {
    Write-Host "RUNNING" -ForegroundColor Green
} else {
    Write-Host "NOT RUNNING" -ForegroundColor Red
}
Write-Host ""

# Step 5: Feature Summary
if ($backendRunning -and $frontendRunning) {
    Write-Host "[5/5] System Status: " -NoNewline -ForegroundColor Yellow
    Write-Host "ALL SYSTEMS OPERATIONAL" -ForegroundColor Green
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  RAG CHATBOT IS READY!" -ForegroundColor White
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Access URLs:" -ForegroundColor White
    Write-Host "  Frontend: http://localhost:8080" -ForegroundColor Cyan
    Write-Host "  Backend:  http://localhost:5000" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Login Credentials:" -ForegroundColor White
    Write-Host "  Username: admin" -ForegroundColor Yellow
    Write-Host "  Password: admin123" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Features Available:" -ForegroundColor White
    Write-Host "  + Tamil Text Display (Noto Sans Tamil font)" -ForegroundColor Green
    Write-Host "  + Tamil Voice Synthesis (Text-to-Speech)" -ForegroundColor Green
    Write-Host "  + Large File Upload (up to 2GB)" -ForegroundColor Green
    Write-Host "  + Enhanced Quiz (Hints, Explanations, References)" -ForegroundColor Green
    Write-Host "  + PDF Processing (Tamil and English)" -ForegroundColor Green
    Write-Host "  + CSV, DOCX, Excel Support" -ForegroundColor Green
    Write-Host "  + Video Transcript Processing" -ForegroundColor Green
    Write-Host "  + Bilingual Chat (Tamil/English)" -ForegroundColor Green
    Write-Host ""
    Write-Host "Test Scenarios:" -ForegroundColor White
    Write-Host "  1. Upload Tamil PDF and ask questions" -ForegroundColor Gray
    Write-Host "  2. Type Tamil text" -ForegroundColor Gray
    Write-Host "  3. Click speaker icon to hear Tamil voice" -ForegroundColor Gray
    Write-Host "  4. Generate quiz with hints" -ForegroundColor Gray
    Write-Host "  5. Upload files up to 2GB" -ForegroundColor Gray
    Write-Host ""
    
    # Open browser
    Start-Sleep -Seconds 2
    Write-Host "Opening browser..." -ForegroundColor Cyan
    Start-Process "http://localhost:8080"
    
} else {
    Write-Host "[5/5] System Status: " -NoNewline -ForegroundColor Yellow
    Write-Host "ISSUES DETECTED" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Check if ports 5000 and 8080 are available" -ForegroundColor Gray
    Write-Host "  2. Verify .NET 8.0 SDK is installed" -ForegroundColor Gray
    Write-Host "  3. Verify Python is installed" -ForegroundColor Gray
    Write-Host "  4. Check Backend console for errors" -ForegroundColor Gray
    Write-Host ""
}

Write-Host ""
Write-Host "Press Enter to keep servers running..." -ForegroundColor Gray
Read-Host
