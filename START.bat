@echo off
cls
echo ========================================
echo   RAG CHATBOT - ALL .NET VERSION
echo ========================================
echo.

echo [1/3] Stopping old processes...
taskkill /F /IM dotnet.exe >nul 2>&1
timeout /t 2 /nobreak >nul

echo [2/3] Building...
cd /d C:\RAGChatbot\Backend\RAGChatbot.API
dotnet build --no-incremental >nul 2>&1

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo [3/3] Starting server...
echo.
echo ========================================
echo   SERVER RUNNING
echo ========================================
echo.
echo   URL: http://localhost:5000
echo   Swagger: http://localhost:5000/swagger
echo.
echo   Open http://localhost:5000 in browser
echo.
echo   Press Ctrl+C to stop
echo ========================================
echo.

dotnet run --urls "http://localhost:5000"
