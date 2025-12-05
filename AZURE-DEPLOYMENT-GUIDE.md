# Azure Deployment Guide for RAG Chatbot

## 📋 Prerequisites

- Azure Account (free tier available: https://azure.microsoft.com/free/)
- Azure CLI installed (https://aka.ms/installazurecli)
- GitHub repository (already created: https://github.com/arulkumarcn-dev/rag-chatbot)
- Valid OpenAI API key

---

## 🚀 Deployment Options

### Option 1: Azure App Service (Recommended for beginners)
### Option 2: Azure Container Apps
### Option 3: Azure Kubernetes Service (AKS)

---

## 📦 Option 1: Deploy to Azure App Service

### Step 1: Install Azure CLI

```powershell
# Check if Azure CLI is installed
az --version

# If not installed, download from: https://aka.ms/installazurecli
```

### Step 2: Login to Azure

```powershell
# Login to your Azure account
az login

# Set your subscription (if you have multiple)
az account list --output table
az account set --subscription "YOUR_SUBSCRIPTION_ID"
```

### Step 3: Create Resource Group

```powershell
# Create a resource group
az group create --name rg-ragchatbot --location eastus
```

### Step 4: Create App Service Plan

```powershell
# Create App Service Plan (B1 tier - suitable for small apps)
az appservice plan create `
  --name plan-ragchatbot `
  --resource-group rg-ragchatbot `
  --sku B1 `
  --is-linux

# For free tier (limited resources):
# az appservice plan create --name plan-ragchatbot --resource-group rg-ragchatbot --sku F1 --is-linux
```

### Step 5: Create Web App

```powershell
# Create Web App for .NET 8
az webapp create `
  --resource-group rg-ragchatbot `
  --plan plan-ragchatbot `
  --name ragchatbot-app `
  --runtime "DOTNETCORE:8.0"

# Note: Replace 'ragchatbot-app' with a unique name (must be globally unique)
```

### Step 6: Configure App Settings

```powershell
# Set environment variables
az webapp config appsettings set `
  --resource-group rg-ragchatbot `
  --name ragchatbot-app `
  --settings `
    OPENAI_API_KEY="your-openai-api-key-here" `
    OPENAI_CHAT_MODEL="gpt-4" `
    OPENAI_EMBEDDING_MODEL="text-embedding-3-small" `
    ASPNETCORE_ENVIRONMENT="Production"
```

### Step 7: Deploy from GitHub

```powershell
# Configure GitHub deployment
az webapp deployment source config `
  --name ragchatbot-app `
  --resource-group rg-ragchatbot `
  --repo-url https://github.com/arulkumarcn-dev/rag-chatbot `
  --branch main `
  --manual-integration
```

### Step 8: Configure Startup Command

```powershell
# Set startup command
az webapp config set `
  --resource-group rg-ragchatbot `
  --name ragchatbot-app `
  --startup-file "dotnet Backend/RAGChatbot.API/RAGChatbot.API.dll"
```

### Step 9: Access Your App

```powershell
# Get the URL
az webapp show --name ragchatbot-app --resource-group rg-ragchatbot --query defaultHostName --output tsv

# Your app will be available at: https://ragchatbot-app.azurewebsites.net
```

---

## 🐳 Option 2: Deploy with Azure Container Apps

### Step 1: Prepare Dockerfile

Create `Dockerfile` in the root of your project:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Backend/RAGChatbot.API/RAGChatbot.API.csproj", "Backend/RAGChatbot.API/"]
RUN dotnet restore "Backend/RAGChatbot.API/RAGChatbot.API.csproj"
COPY . .
WORKDIR "/src/Backend/RAGChatbot.API"
RUN dotnet build "RAGChatbot.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RAGChatbot.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY Frontend-HTML ./Frontend-HTML
ENTRYPOINT ["dotnet", "RAGChatbot.API.dll"]
```

### Step 2: Build and Push to Azure Container Registry

```powershell
# Create Azure Container Registry
az acr create `
  --resource-group rg-ragchatbot `
  --name ragchatbotacr `
  --sku Basic `
  --admin-enabled true

# Login to ACR
az acr login --name ragchatbotacr

# Build and push image
az acr build `
  --registry ragchatbotacr `
  --image ragchatbot:latest `
  --file Dockerfile .
```

### Step 3: Create Container App Environment

```powershell
# Install Container Apps extension
az extension add --name containerapp --upgrade

# Create Container Apps environment
az containerapp env create `
  --name ragchatbot-env `
  --resource-group rg-ragchatbot `
  --location eastus
```

### Step 4: Deploy Container App

```powershell
# Get ACR credentials
$ACR_USERNAME = az acr credential show --name ragchatbotacr --query username --output tsv
$ACR_PASSWORD = az acr credential show --name ragchatbotacr --query passwords[0].value --output tsv

# Create Container App
az containerapp create `
  --name ragchatbot-container `
  --resource-group rg-ragchatbot `
  --environment ragchatbot-env `
  --image ragchatbotacr.azurecr.io/ragchatbot:latest `
  --registry-server ragchatbotacr.azurecr.io `
  --registry-username $ACR_USERNAME `
  --registry-password $ACR_PASSWORD `
  --target-port 5000 `
  --ingress external `
  --env-vars `
    OPENAI_API_KEY="your-openai-api-key-here" `
    ASPNETCORE_ENVIRONMENT="Production"
```

---

## ⚙️ Option 3: Manual Deployment (FTP/ZIP)

### Step 1: Publish Locally

```powershell
cd C:\RAGChatbot\Backend\RAGChatbot.API

# Publish the application
dotnet publish -c Release -o .\publish

# Copy Frontend files
Copy-Item -Path ..\..\Frontend-HTML -Destination .\publish\Frontend-HTML -Recurse
```

### Step 2: Create ZIP File

```powershell
# Create deployment package
Compress-Archive -Path .\publish\* -DestinationPath .\ragchatbot-deploy.zip -Force
```

### Step 3: Deploy via Azure CLI

```powershell
# Deploy ZIP file
az webapp deployment source config-zip `
  --resource-group rg-ragchatbot `
  --name ragchatbot-app `
  --src .\ragchatbot-deploy.zip
```

---

## 🔧 Post-Deployment Configuration

### 1. Enable HTTPS Only

```powershell
az webapp update `
  --resource-group rg-ragchatbot `
  --name ragchatbot-app `
  --https-only true
```

### 2. Configure CORS (if needed)

```powershell
az webapp cors add `
  --resource-group rg-ragchatbot `
  --name ragchatbot-app `
  --allowed-origins "*"
```

### 3. Add Custom Domain (Optional)

```powershell
# Add custom domain
az webapp config hostname add `
  --webapp-name ragchatbot-app `
  --resource-group rg-ragchatbot `
  --hostname www.yourdomain.com

# Enable SSL
az webapp config ssl bind `
  --name ragchatbot-app `
  --resource-group rg-ragchatbot `
  --certificate-thumbprint YOUR_CERT_THUMBPRINT `
  --ssl-type SNI
```

### 4. Set Up Azure Storage for Vector Store

```powershell
# Create storage account
az storage account create `
  --name ragchatbotstorage `
  --resource-group rg-ragchatbot `
  --location eastus `
  --sku Standard_LRS

# Get connection string
az storage account show-connection-string `
  --name ragchatbotstorage `
  --resource-group rg-ragchatbot `
  --output tsv

# Add to app settings
az webapp config appsettings set `
  --resource-group rg-ragchatbot `
  --name ragchatbot-app `
  --settings AZURE_STORAGE_CONNECTION_STRING="your-connection-string"
```

---

## 📊 Monitoring and Logging

### Enable Application Insights

```powershell
# Create Application Insights
az monitor app-insights component create `
  --app ragchatbot-insights `
  --location eastus `
  --resource-group rg-ragchatbot `
  --application-type web

# Get instrumentation key
$INSTRUMENTATION_KEY = az monitor app-insights component show `
  --app ragchatbot-insights `
  --resource-group rg-ragchatbot `
  --query instrumentationKey `
  --output tsv

# Add to app settings
az webapp config appsettings set `
  --resource-group rg-ragchatbot `
  --name ragchatbot-app `
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=$INSTRUMENTATION_KEY"
```

### View Logs

```powershell
# Stream logs
az webapp log tail `
  --resource-group rg-ragchatbot `
  --name ragchatbot-app

# Download logs
az webapp log download `
  --resource-group rg-ragchatbot `
  --name ragchatbot-app `
  --log-file logs.zip
```

---

## 🔄 CI/CD with GitHub Actions

### Step 1: Create GitHub Workflow

Create `.github/workflows/azure-deploy.yml`:

```yaml
name: Deploy to Azure

on:
  push:
    branches: [ main ]
  workflow_dispatch:

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore Backend/RAGChatbot.API/RAGChatbot.API.csproj
    
    - name: Build
      run: dotnet build Backend/RAGChatbot.API/RAGChatbot.API.csproj -c Release --no-restore
    
    - name: Publish
      run: dotnet publish Backend/RAGChatbot.API/RAGChatbot.API.csproj -c Release -o ./publish
    
    - name: Copy Frontend
      run: cp -r Frontend-HTML ./publish/
    
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: ragchatbot-app
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

### Step 2: Configure GitHub Secrets

```powershell
# Get publish profile
az webapp deployment list-publishing-profiles `
  --resource-group rg-ragchatbot `
  --name ragchatbot-app `
  --xml
```

Add the output as `AZURE_WEBAPP_PUBLISH_PROFILE` in GitHub Secrets:
- Go to: https://github.com/arulkumarcn-dev/rag-chatbot/settings/secrets/actions
- Click "New repository secret"
- Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
- Value: Paste the XML content

---

## 💰 Cost Estimation

### App Service (B1 Plan)
- Monthly: ~$13 USD
- Includes: 1 core, 1.75 GB RAM, 10 GB storage

### Free Tier (F1)
- Monthly: $0
- Limitations: 60 CPU minutes/day, 1 GB RAM

### Container Apps
- Monthly: ~$15-30 USD (varies with usage)
- Pay-per-use model

### Storage Account
- Monthly: ~$1-5 USD (depends on data volume)

---

## 🔐 Security Best Practices

1. **Always use Key Vault for secrets:**
```powershell
# Create Key Vault
az keyvault create `
  --name ragchatbot-vault `
  --resource-group rg-ragchatbot `
  --location eastus

# Add secret
az keyvault secret set `
  --vault-name ragchatbot-vault `
  --name OpenAIKey `
  --value "your-openai-api-key"

# Grant Web App access
az webapp identity assign `
  --resource-group rg-ragchatbot `
  --name ragchatbot-app

$PRINCIPAL_ID = az webapp identity show `
  --resource-group rg-ragchatbot `
  --name ragchatbot-app `
  --query principalId `
  --output tsv

az keyvault set-policy `
  --name ragchatbot-vault `
  --object-id $PRINCIPAL_ID `
  --secret-permissions get list
```

2. **Enable Authentication:**
```powershell
az webapp auth update `
  --resource-group rg-ragchatbot `
  --name ragchatbot-app `
  --enabled true `
  --action LoginWithAzureActiveDirectory
```

3. **Set up firewall rules:**
```powershell
az webapp config access-restriction add `
  --resource-group rg-ragchatbot `
  --name ragchatbot-app `
  --rule-name AllowMyIP `
  --action Allow `
  --ip-address YOUR_IP_ADDRESS `
  --priority 100
```

---

## 🧪 Testing Your Deployment

```powershell
# Test the endpoint
$APP_URL = az webapp show `
  --name ragchatbot-app `
  --resource-group rg-ragchatbot `
  --query defaultHostName `
  --output tsv

# Open in browser
Start-Process "https://$APP_URL"

# Test API
Invoke-WebRequest -Uri "https://$APP_URL/api/chat/test" -Method GET
```

---

## 🐛 Troubleshooting

### View Application Logs
```powershell
az webapp log tail --resource-group rg-ragchatbot --name ragchatbot-app
```

### Restart Application
```powershell
az webapp restart --resource-group rg-ragchatbot --name ragchatbot-app
```

### Check App Status
```powershell
az webapp show --resource-group rg-ragchatbot --name ragchatbot-app --query state
```

### SSH into Container (for debugging)
```powershell
az webapp ssh --resource-group rg-ragchatbot --name ragchatbot-app
```

---

## 🗑️ Cleanup Resources

```powershell
# Delete entire resource group (removes all resources)
az group delete --name rg-ragchatbot --yes --no-wait
```

---

## 📚 Additional Resources

- [Azure App Service Documentation](https://docs.microsoft.com/azure/app-service/)
- [Azure Container Apps Documentation](https://docs.microsoft.com/azure/container-apps/)
- [Azure Pricing Calculator](https://azure.microsoft.com/pricing/calculator/)
- [Azure Free Account](https://azure.microsoft.com/free/)

---

## 🎯 Quick Start Commands

```powershell
# Complete deployment in one script
cd C:\RAGChatbot

# Login
az login

# Create and deploy
az group create --name rg-ragchatbot --location eastus
az appservice plan create --name plan-ragchatbot --resource-group rg-ragchatbot --sku B1 --is-linux
az webapp create --resource-group rg-ragchatbot --plan plan-ragchatbot --name ragchatbot-app --runtime "DOTNETCORE:8.0"
az webapp config appsettings set --resource-group rg-ragchatbot --name ragchatbot-app --settings OPENAI_API_KEY="your-key-here"
az webapp deployment source config --name ragchatbot-app --resource-group rg-ragchatbot --repo-url https://github.com/arulkumarcn-dev/rag-chatbot --branch main --manual-integration

# Get URL
az webapp show --name ragchatbot-app --resource-group rg-ragchatbot --query defaultHostName --output tsv
```

---

**Your app will be live at:** `https://ragchatbot-app.azurewebsites.net`

🎉 **Congratulations! Your RAG Chatbot is now deployed to Azure!**
