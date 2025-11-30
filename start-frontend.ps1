# RAG Chatbot - Start Frontend Script

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RAG Chatbot - Starting Frontend     " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Node.js is installed
try {
    $nodeVersion = node --version 2>$null
    Write-Host "✓ Node.js detected: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ ERROR: Node.js is not installed!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install Node.js from: https://nodejs.org/" -ForegroundColor Yellow
    Write-Host "Recommended version: LTS (Long Term Support)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "After installation:" -ForegroundColor Cyan
    Write-Host "1. Close and reopen PowerShell" -ForegroundColor Cyan
    Write-Host "2. Run this script again" -ForegroundColor Cyan
    Write-Host ""
    
    $openBrowser = Read-Host "Open Node.js download page in browser? (y/n)"
    if ($openBrowser -eq "y") {
        Start-Process "https://nodejs.org/"
    }
    exit 1
}

# Navigate to frontend directory
Set-Location -Path "$PSScriptRoot\Frontend"

# Check if node_modules exists
if (-Not (Test-Path "node_modules")) {
    Write-Host "Installing dependencies (this may take a few minutes)..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to install dependencies" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "Starting React development server..." -ForegroundColor Green
Write-Host "The app will open automatically at: http://localhost:3000" -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Gray
Write-Host ""

# Start the development server
npm start
