# Simple HTTP Server for Frontend
# This serves the HTML frontend on port 8081

$port = 9000
$directory = "C:\RAGChatbot\Frontend-HTML"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Starting Frontend Web Server" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Directory: $directory" -ForegroundColor Gray
Write-Host "Local URL:  http://localhost:$port" -ForegroundColor Green
Write-Host ""
Write-Host "Press Ctrl+C to stop" -ForegroundColor Yellow
Write-Host ""

# Simple HTTP listener
$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add("http://localhost:$port/")
$listener.Start()

Write-Host "Server is running..." -ForegroundColor Green
Write-Host "Open: http://localhost:$port" -ForegroundColor Cyan
Write-Host ""

try {
    while ($listener.IsListening) {
        $context = $listener.GetContext()
        $request = $context.Request
        $response = $context.Response
        
        $path = $request.Url.LocalPath
        if ($path -eq "/" -or $path -eq "") {
            $path = "/index.html"
        }
        
        $filePath = Join-Path $directory $path.TrimStart('/')
        
        Write-Host "Request: $path" -ForegroundColor Gray
        
        if (Test-Path $filePath) {
            $content = [System.IO.File]::ReadAllBytes($filePath)
            
            # Set content type
            $extension = [System.IO.Path]::GetExtension($filePath)
            $contentType = switch ($extension) {
                ".html" { "text/html" }
                ".css"  { "text/css" }
                ".js"   { "application/javascript" }
                ".json" { "application/json" }
                ".png"  { "image/png" }
                ".jpg"  { "image/jpeg" }
                ".jpeg" { "image/jpeg" }
                default { "application/octet-stream" }
            }
            
            $response.ContentType = $contentType
            $response.ContentLength64 = $content.Length
            $response.OutputStream.Write($content, 0, $content.Length)
        }
        else {
            $response.StatusCode = 404
            $buffer = [System.Text.Encoding]::UTF8.GetBytes("404 - File Not Found")
            $response.ContentLength64 = $buffer.Length
            $response.OutputStream.Write($buffer, 0, $buffer.Length)
        }
        
        $response.OutputStream.Close()
    }
}
finally {
    $listener.Stop()
}
