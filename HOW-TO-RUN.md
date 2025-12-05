# 🚀 How to Run the RAG Chatbot

## Quick Start - Choose One Method:

### Method 1: Automated Script (Easiest)

```powershell
cd C:\RAGChatbot
.\START.ps1
```

### Method 2: Manual Steps

**Step 1 - Start Backend:**
```powershell
cd C:\RAGChatbot\Backend\RAGChatbot.API
dotnet run --urls "http://localhost:5000"
```
Keep this window open!

**Step 2 - Open Frontend (in a NEW PowerShell window):**
```powershell
Start-Process "C:\RAGChatbot\Frontend-HTML\index.html"
```

Or just double-click `C:\RAGChatbot\Frontend-HTML\index.html`

---

## Common PowerShell Commands Explained:

### Correct Way:
```powershell
# Navigate TO a folder
cd C:\RAGChatbot

# Run a script
.\START.ps1

# Run a script with full path
& "C:\RAGChatbot\START.ps1"
```

### Wrong Way:
```powershell
# ❌ This doesn't work - can't cd to a file
cd C:\RAGChatbot\START.ps1
```

---

## Troubleshooting:

### "Cannot run scripts on this system"

Run this once in PowerShell (as Administrator):
```powershell
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
```

Then try again:
```powershell
cd C:\RAGChatbot
.\START.ps1
```

### Port Already in Use

Kill the process:
```powershell
$pid = (Get-NetTCPConnection -LocalPort 5000).OwningProcess
Stop-Process -Id $pid -Force
```

Then restart:
```powershell
.\START.ps1
```

### Backend Won't Start

```powershell
cd C:\RAGChatbot\Backend\RAGChatbot.API
dotnet clean
dotnet restore
dotnet build
dotnet run --urls "http://localhost:5000"
```

---

## What Each Command Does:

| Command | What It Does |
|---------|--------------|
| `cd C:\RAGChatbot` | Changes directory to RAGChatbot folder |
| `.\START.ps1` | Runs the START.ps1 script in current folder |
| `Start-Process` | Opens a file or starts a program |
| `dotnet run` | Runs the .NET application |

---

## Need the Full Guide?

Read: `C:\RAGChatbot\COMPLETE-GUIDE.md`

Or open it:
```powershell
notepad C:\RAGChatbot\COMPLETE-GUIDE.md
```
