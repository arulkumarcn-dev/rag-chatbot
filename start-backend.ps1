# RAG Chatbot - Start Backend Script

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RAG Chatbot - Starting Backend API  " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Navigate to backend directory
Set-Location -Path "$PSScriptRoot\Backend\RAGChatbot.API"

# Check if appsettings.json has API key configured
$appSettings = Get-Content "appsettings.json" | ConvertFrom-Json
if ($appSettings.OpenAI.ApiKey -eq "your-openai-api-key-here") {
    Write-Host "⚠️  WARNING: OpenAI API key not configured!" -ForegroundColor Yellow
    Write-Host "Please edit Backend\RAGChatbot.API\appsettings.json and add your API key" -ForegroundColor Yellow
    Write-Host ""
    $continue = Read-Host "Continue anyway? (y/n)"
    if ($continue -ne "y") {
        exit
    }
}

# Kill any existing process on port 5000
Write-Host "Checking for existing processes on port 5000..." -ForegroundColor Gray
$existingProcess = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue
if ($existingProcess) {
    $processId = $existingProcess.OwningProcess
    Write-Host "Stopping existing process (PID: $processId)..." -ForegroundColor Yellow
    Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
}

Write-Host "Starting backend API on http://localhost:5000..." -ForegroundColor Green
Write-Host ""
Write-Host "Swagger UI will be available at: http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Gray
Write-Host ""

# Run the application
dotnet run --urls "http://localhost:5000"
