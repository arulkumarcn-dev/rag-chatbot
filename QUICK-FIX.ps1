# Quick fix script to add error handling

Write-Host "Applying quick fix to ChatService..." -ForegroundColor Yellow

$chatServicePath = "C:\RAGChatbot\Backend\RAGChatbot.API\Services\ChatService.cs"

# Read the current file
$content = Get-Content $chatServicePath -Raw

# Replace the problematic constructor line
$oldInit = "        // Initialize vector store`r`n        _vectorStore.InitializeAsync().Wait();"
$newInit = @"
        // Initialize vector store
        try
        {
            _vectorStore.InitializeAsync().Wait();
            _logger.LogInformation("Vector store initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize vector store - it will be initialized on first use");
        }
"@

$content = $content -replace [regex]::Escape($oldInit), $newInit

# Save the file
Set-Content $chatServicePath -Value $content -NoNewline

Write-Host "Fixed! Now rebuilding..." -ForegroundColor Green

cd C:\RAGChatbot\Backend\RAGChatbot.API

# Clean build
Remove-Item bin\Debug\net8.0\* -Recurse -Force -ErrorAction SilentlyContinue
dotnet build --no-incremental

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ Build successful!" -ForegroundColor Green
    Write-Host "`nNow restart the backend:" -ForegroundColor Yellow
    Write-Host "1. Close the old Backend Server window" -ForegroundColor Cyan
    Write-Host "2. Run: cd C:\RAGChatbot\Backend\RAGChatbot.API; dotnet run --urls 'http://localhost:5000'" -ForegroundColor Cyan
} else {
    Write-Host "`n❌ Build failed - check errors above" -ForegroundColor Red
}
