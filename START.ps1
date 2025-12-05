# Quick Start Script - RAG Chatbot
# This script helps you start the entire system

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "    RAG Chatbot - Quick Start" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Kill any existing processes on the ports
Write-Host "Cleaning up existing processes..." -ForegroundColor Yellow
$port5000 = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
if ($port5000) {
    $port5000 | ForEach-Object { Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue }
    Write-Host "  Stopped process on port 5000" -ForegroundColor Gray
}
$port8080 = Get-NetTCPConnection -LocalPort 8080 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
if ($port8080) {
    $port8080 | ForEach-Object { Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue }
    Write-Host "  Stopped process on port 8080" -ForegroundColor Gray
}

# Check .NET is installed
Write-Host ""
Write-Host "Checking prerequisites..." -ForegroundColor Yellow

try {
    $dotnetVersion = dotnet --version
    Write-Host "  .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "Error: .NET SDK not found!" -ForegroundColor Red
    Write-Host "   Please install from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit
}

# Check API Key is configured
Write-Host ""
Write-Host "Checking API key configuration..." -ForegroundColor Yellow
$appsettingsPath = "C:\RAGChatbot\Backend\RAGChatbot.API\appsettings.json"

if (Test-Path $appsettingsPath) {
    $content = Get-Content $appsettingsPath -Raw
    if ($content -match '"ApiKey":\s*"your-openai-api-key-here"') {
        Write-Host "Warning: API Key not configured!" -ForegroundColor Yellow
        Write-Host "   Please edit: $appsettingsPath" -ForegroundColor Yellow
        Write-Host "   Add your OpenAI API key" -ForegroundColor Yellow
        Write-Host ""
        $continue = Read-Host "Continue anyway? (y/n)"
        if ($continue -ne 'y' -and $continue -ne 'Y') {
            exit
        }
    } else {
        Write-Host "Success: API Key configured" -ForegroundColor Green
    }
} else {
    Write-Host "Error: appsettings.json not found at: $appsettingsPath" -ForegroundColor Red
    exit
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "    Starting Backend..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if port 5000 is already in use
$portInUse = $false
try {
    $connection = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue
    if ($connection) {
        $portInUse = $true
        Write-Host "Warning: Port 5000 is already in use!" -ForegroundColor Yellow
        Write-Host ""
        $kill = Read-Host "Kill the existing process? (y/n)"
        
        if ($kill -eq 'y' -or $kill -eq 'Y') {
            $pid = $connection.OwningProcess
            Stop-Process -Id $pid -Force
            Write-Host "Success: Process killed" -ForegroundColor Green
            Start-Sleep -Seconds 2
        } else {
            Write-Host "Using existing backend process..." -ForegroundColor Yellow
        }
    }
} catch {
    # Port not in use, continue
}

# Start backend if not already running
if (-not $portInUse -or $kill -eq 'y' -or $kill -eq 'Y') {
    Write-Host "Starting backend server..." -ForegroundColor Green
    Write-Host ""
    
    $backendPath = "C:\RAGChatbot\Backend\RAGChatbot.API"
    
    # Start backend in new window
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$backendPath'; Write-Host 'RAG Chatbot Backend' -ForegroundColor Cyan; Write-Host 'Keep this window open while using the app' -ForegroundColor Yellow; Write-Host ''; dotnet run --urls 'http://localhost:5000'"
    
    Write-Host "Waiting for backend to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
    
    # Verify backend started
    $retries = 0
    $maxRetries = 10
    $backendReady = $false
    
    while ($retries -lt $maxRetries -and -not $backendReady) {
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:5000" -Method HEAD -TimeoutSec 2 -ErrorAction SilentlyContinue
            $backendReady = $true
            Write-Host "Success: Backend is ready!" -ForegroundColor Green
        } catch {
            $retries++
            Write-Host "   Waiting... ($retries/$maxRetries)" -ForegroundColor Gray
            Start-Sleep -Seconds 2
        }
    }
    
    if (-not $backendReady) {
        Write-Host "Warning: Backend may not be ready yet" -ForegroundColor Yellow
        Write-Host "   Check the backend window for errors" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "    Starting Frontend Server..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$frontendServerPath = "C:\RAGChatbot\start-frontend-server.ps1"

if (Test-Path $frontendServerPath) {
    Write-Host "Starting HTTP server on port 8080..." -ForegroundColor Yellow
    Start-Process powershell -ArgumentList "-NoExit", "-ExecutionPolicy", "Bypass", "-File", $frontendServerPath
    Start-Sleep -Seconds 3
    
    # Verify frontend server started
    $frontendConn = Get-NetTCPConnection -LocalPort 8080 -ErrorAction SilentlyContinue
    if ($frontendConn) {
        Write-Host "Success: Frontend server is running!" -ForegroundColor Green
        Write-Host "Opening browser..." -ForegroundColor Yellow
        Start-Process "http://localhost:8080"
        Start-Sleep -Seconds 2
    } else {
        Write-Host "Warning: Frontend server may not have started" -ForegroundColor Yellow
    }
} else {
    Write-Host "Warning: Frontend server script not found" -ForegroundColor Yellow
    Write-Host "   Trying to open HTML directly..." -ForegroundColor Gray
    $htmlPath = "C:\RAGChatbot\Frontend-HTML\index.html"
    if (Test-Path $htmlPath) {
        Start-Process $htmlPath
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "    System Started!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "What is Running:" -ForegroundColor White
Write-Host "   Backend API:  http://localhost:5000" -ForegroundColor Cyan
Write-Host "   Frontend:     http://localhost:8080" -ForegroundColor Cyan
Write-Host "   Swagger Docs: http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor White
Write-Host "   1. Upload a document or video transcript" -ForegroundColor Gray
Write-Host "   2. Start chatting with your data!" -ForegroundColor Gray
Write-Host "   3. Type exit to end the chat session" -ForegroundColor Gray
Write-Host ""
Write-Host "Need Help?" -ForegroundColor White
Write-Host "   Read the complete guide: COMPLETE-GUIDE.md" -ForegroundColor Gray
Write-Host ""
Write-Host "To Stop:" -ForegroundColor White
Write-Host "   Close the backend PowerShell window" -ForegroundColor Gray
Write-Host "   Or press Ctrl+C in the backend window" -ForegroundColor Gray
Write-Host ""
Write-Host "Press Enter to close this window..." -ForegroundColor Yellow
Read-Host
