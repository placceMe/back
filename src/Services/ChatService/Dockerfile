# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy solution file if exists (optional)
COPY *.sln ./

# Copy project files for better layer caching
COPY src/Services/ChatService/ChatService.csproj src/Services/ChatService/
COPY src/Shared/Marketplace.Contracts/Marketplace.Contracts.csproj src/Shared/Marketplace.Contracts/

# Restore dependencies with retry and cache clear
RUN dotnet nuget locals all --clear && \
    (dotnet restore src/Services/ChatService/ChatService.csproj --no-cache --disable-parallel || \
     (sleep 5 && dotnet restore src/Services/ChatService/ChatService.csproj --no-cache --disable-parallel) || \
     (sleep 10 && dotnet restore src/Services/ChatService/ChatService.csproj --no-cache --disable-parallel))

# Copy all source code
COPY . .

# Build and publish
WORKDIR /src/src/Services/ChatService
RUN dotnet publish ChatService.csproj -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "ChatService.dll"]