@echo off
title RAG Chatbot - Complete Start
color 0A
cls

echo ========================================
echo   RAG CHATBOT - COMPLETE START
echo ========================================
echo.
echo Checking servers...
echo.

REM Check if backend is running
netstat -ano | findstr ":5000" | findstr "LISTENING" >nul
if %errorlevel% neq 0 (
    echo [1/2] Starting Backend API...
    start "RAG-BACKEND" cmd /k "cd Backend\RAGChatbot.API && color 0A && dotnet run"
    timeout /t 10 /nobreak >nul
    echo       Backend started on port 5000
) else (
    echo [1/2] Backend already running on port 5000
)

REM Check if frontend is running
netstat -ano | findstr ":8080" | findstr "LISTENING" >nul
if %errorlevel% neq 0 (
    echo [2/2] Starting Frontend Server...
    start "RAG-FRONTEND" cmd /k "cd Frontend-HTML && color 0B && python -m http.server 8080"
    timeout /t 3 /nobreak >nul
    echo       Frontend started on port 8080
) else (
    echo [2/2] Frontend already running on port 8080
)

echo.
echo ========================================
echo   SERVERS ARE RUNNING!
echo ========================================
echo.
echo Login Page: http://localhost:8080/login.html
echo Main App:   http://localhost:8080/index.html
echo.
echo Login: admin / admin123
echo.
timeout /t 3 /nobreak >nul

echo Opening login page in browser...
start http://localhost:8080/login.html

echo.
echo ========================================
echo   READY TO USE!
echo ========================================
echo.
echo Instructions:
echo 1. Login page should open in browser
echo 2. Username: admin
echo 3. Password: admin123
echo 4. Click Login button
echo 5. You will be redirected to main app
echo.
echo Servers are running in separate windows.
echo Close those windows to stop the servers.
echo.
pause
