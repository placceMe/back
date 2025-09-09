# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY ["src/Shared/Marketplace.Authentication/Marketplace.Authentication.csproj", "src/Shared/Marketplace.Authentication/"]
COPY ["src/Shared/Marketplace.Contracts/Marketplace.Contracts.csproj", "src/Shared/Marketplace.Contracts/"]

# Copy project file
COPY ["src/Services/ContentService/ContentService.csproj", "src/Services/ContentService/"]

# Restore dependencies
RUN dotnet restore "src/Services/ContentService/ContentService.csproj"

# Copy everything else
COPY . .

# Build the application
WORKDIR "/src/src/Services/ContentService"
RUN dotnet build "ContentService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ContentService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "ContentService.dll"]
