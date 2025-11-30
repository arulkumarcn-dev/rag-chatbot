# .NET Backend - Quick Start Guide

## Prerequisites
- .NET 8.0 SDK or later
- OpenAI API Key

## Installation

1. Navigate to the backend directory:
```powershell
cd Backend\RAGChatbot.API
```

2. Restore dependencies:
```powershell
dotnet restore
```

3. Configure API Keys:
   - Open `appsettings.json`
   - Replace `your-openai-api-key-here` with your actual OpenAI API key

4. Run the application:
```powershell
dotnet run --urls "http://localhost:5000"
```

## Testing

Access Swagger UI at: `http://localhost:5000/swagger`

## Environment Variables (Alternative to appsettings.json)

You can also set these as environment variables:

```powershell
$env:OpenAI__ApiKey = "your-api-key"
$env:OpenAI__ChatModel = "gpt-4"
$env:OpenAI__EmbeddingModel = "text-embedding-3-small"
```

## Troubleshooting

### Port Already in Use
If port 5000 is in use, change it:
```powershell
dotnet run --urls "http://localhost:5001"
```

### Missing Dependencies
If you get package errors:
```powershell
dotnet clean
dotnet restore
dotnet build
```

## Production Deployment

For production, use:
```powershell
dotnet publish -c Release -o ./publish
cd publish
dotnet RAGChatbot.API.dll
```
