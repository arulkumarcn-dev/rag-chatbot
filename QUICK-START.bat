@echo off
echo ========================================
echo   RAG CHATBOT - QUICK START
echo ========================================
echo.

echo [1/2] Starting Backend API...
start "RAG BACKEND" cmd /k "cd Backend\RAGChatbot.API && dotnet run"
timeout /t 8 /nobreak >nul

echo [2/2] Starting Frontend Server...
start "RAG FRONTEND" cmd /k "cd Frontend-HTML && python -m http.server 8080"
timeout /t 3 /nobreak >nul

echo.
echo ========================================
echo   SERVERS STARTED!
echo ========================================
echo.
echo Frontend: http://localhost:8080
echo Backend:  http://localhost:5000
echo.
echo Login: admin / admin123
echo.
echo Press any key to open browser...
pause >nul

start http://localhost:8080

echo.
echo Servers are running in separate windows
echo Close those windows to stop the servers
echo.
pause
