# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Backend/RAGChatbot.API/RAGChatbot.API.csproj", "Backend/RAGChatbot.API/"]
RUN dotnet restore "Backend/RAGChatbot.API/RAGChatbot.API.csproj"

# Copy everything else and build
COPY Backend/ Backend/
WORKDIR "/src/Backend/RAGChatbot.API"
RUN dotnet build "RAGChatbot.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "RAGChatbot.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Copy frontend files
COPY Frontend-HTML ./Frontend-HTML

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080

# Run the application
ENTRYPOINT ["dotnet", "RAGChatbot.API.dll"]
