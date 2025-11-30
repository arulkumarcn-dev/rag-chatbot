Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  OpenAI API Key Setup" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Your backend is running but needs an OpenAI API key to work." -ForegroundColor White
Write-Host ""
Write-Host "Steps to get your API key:" -ForegroundColor Yellow
Write-Host "1. Go to: https://platform.openai.com/api-keys" -ForegroundColor Cyan
Write-Host "2. Sign in or create an account" -ForegroundColor White
Write-Host "3. Click 'Create new secret key'" -ForegroundColor White
Write-Host "4. Copy the key (starts with 'sk-')" -ForegroundColor White
Write-Host ""
Write-Host "Enter your OpenAI API key (or press Ctrl+C to cancel):" -ForegroundColor Yellow
$apiKey = Read-Host

if ([string]::IsNullOrWhiteSpace($apiKey)) {
    Write-Host "No API key provided. Exiting..." -ForegroundColor Red
    exit
}

# Update appsettings.json
$settingsPath = "C:\RAGChatbot\Backend\RAGChatbot.API\appsettings.json"
$settings = Get-Content $settingsPath -Raw | ConvertFrom-Json
$settings.OpenAI.ApiKey = $apiKey
$settings | ConvertTo-Json -Depth 10 | Set-Content $settingsPath

Write-Host ""
Write-Host "✓ API key saved successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Now restart the backend:" -ForegroundColor Yellow
Write-Host "1. Close the backend PowerShell window" -ForegroundColor White
Write-Host "2. Run: cd C:\RAGChatbot\Backend\RAGChatbot.API" -ForegroundColor Cyan
Write-Host "3. Run: dotnet run --urls 'http://localhost:5000'" -ForegroundColor Cyan
Write-Host ""
