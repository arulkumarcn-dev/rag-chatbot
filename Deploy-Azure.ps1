# Azure Deployment Script for RAG Chatbot
# Run this script to deploy your application to Azure

param(
    [Parameter(Mandatory=$true)]
    [string]$AppName,
    
    [Parameter(Mandatory=$true)]
    [string]$OpenAIKey,
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroup = "rg-ragchatbot",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "eastus",
    
    [Parameter(Mandatory=$false)]
    [string]$Sku = "B1"
)

Write-Host "🚀 Starting Azure Deployment..." -ForegroundColor Green
Write-Host ""

# Step 1: Check Azure CLI
Write-Host "1️⃣ Checking Azure CLI..." -ForegroundColor Yellow
try {
    az --version | Out-Null
    Write-Host "✅ Azure CLI is installed" -ForegroundColor Green
} catch {
    Write-Host "❌ Azure CLI not found. Please install from: https://aka.ms/installazurecli" -ForegroundColor Red
    exit 1
}

# Step 2: Login to Azure
Write-Host ""
Write-Host "2️⃣ Logging in to Azure..." -ForegroundColor Yellow
az login

# Step 3: Create Resource Group
Write-Host ""
Write-Host "3️⃣ Creating Resource Group: $ResourceGroup..." -ForegroundColor Yellow
az group create --name $ResourceGroup --location $Location
Write-Host "✅ Resource Group created" -ForegroundColor Green

# Step 4: Create App Service Plan
Write-Host ""
Write-Host "4️⃣ Creating App Service Plan..." -ForegroundColor Yellow
$PlanName = "plan-$AppName"
az appservice plan create `
    --name $PlanName `
    --resource-group $ResourceGroup `
    --sku $Sku `
    --is-linux
Write-Host "✅ App Service Plan created" -ForegroundColor Green

# Step 5: Create Web App
Write-Host ""
Write-Host "5️⃣ Creating Web App: $AppName..." -ForegroundColor Yellow
az webapp create `
    --resource-group $ResourceGroup `
    --plan $PlanName `
    --name $AppName `
    --runtime "DOTNETCORE:8.0"
Write-Host "✅ Web App created" -ForegroundColor Green

# Step 6: Configure App Settings
Write-Host ""
Write-Host "6️⃣ Configuring App Settings..." -ForegroundColor Yellow
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $AppName `
    --settings `
        OPENAI_API_KEY="$OpenAIKey" `
        OPENAI_CHAT_MODEL="gpt-4" `
        OPENAI_EMBEDDING_MODEL="text-embedding-3-small" `
        ASPNETCORE_ENVIRONMENT="Production" `
        ASPNETCORE_URLS="http://+:8080"
Write-Host "✅ App Settings configured" -ForegroundColor Green

# Step 7: Enable HTTPS
Write-Host ""
Write-Host "7️⃣ Enabling HTTPS..." -ForegroundColor Yellow
az webapp update `
    --resource-group $ResourceGroup `
    --name $AppName `
    --https-only true
Write-Host "✅ HTTPS enabled" -ForegroundColor Green

# Step 8: Build and Publish
Write-Host ""
Write-Host "8️⃣ Building and Publishing Application..." -ForegroundColor Yellow
$PublishPath = ".\Backend\RAGChatbot.API\publish"
dotnet publish .\Backend\RAGChatbot.API\RAGChatbot.API.csproj -c Release -o $PublishPath

# Copy Frontend
Copy-Item -Path .\Frontend-HTML -Destination "$PublishPath\Frontend-HTML" -Recurse -Force
Write-Host "✅ Application built" -ForegroundColor Green

# Step 9: Create ZIP
Write-Host ""
Write-Host "9️⃣ Creating deployment package..." -ForegroundColor Yellow
$ZipPath = ".\ragchatbot-deploy.zip"
if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
}
Compress-Archive -Path "$PublishPath\*" -DestinationPath $ZipPath -Force
Write-Host "✅ Deployment package created" -ForegroundColor Green

# Step 10: Deploy
Write-Host ""
Write-Host "🔟 Deploying to Azure..." -ForegroundColor Yellow
az webapp deployment source config-zip `
    --resource-group $ResourceGroup `
    --name $AppName `
    --src $ZipPath
Write-Host "✅ Deployment complete" -ForegroundColor Green

# Get App URL
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🎉 Deployment Successful!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
$AppUrl = az webapp show --name $AppName --resource-group $ResourceGroup --query defaultHostName --output tsv
Write-Host ""
Write-Host "🌐 Your application is live at:" -ForegroundColor Yellow
Write-Host "   https://$AppUrl" -ForegroundColor Cyan
Write-Host ""
Write-Host "📊 To view logs, run:" -ForegroundColor Yellow
Write-Host "   az webapp log tail --resource-group $ResourceGroup --name $AppName" -ForegroundColor Cyan
Write-Host ""
Write-Host "🗑️  To delete resources, run:" -ForegroundColor Yellow
Write-Host "   az group delete --name $ResourceGroup --yes --no-wait" -ForegroundColor Cyan
Write-Host ""

# Clean up local files
Remove-Item $PublishPath -Recurse -Force
Remove-Item $ZipPath -Force

# Open in browser
$OpenBrowser = Read-Host "Open app in browser? (Y/N)"
if ($OpenBrowser -eq "Y" -or $OpenBrowser -eq "y") {
    Start-Process "https://$AppUrl"
}
