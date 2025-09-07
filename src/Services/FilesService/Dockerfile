# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy solution file if exists (optional)
COPY *.sln ./

# Copy project files for better layer caching
COPY src/Services/FilesService/FilesService.csproj src/Services/FilesService/
COPY src/Shared/Marketplace.Contracts/Marketplace.Contracts.csproj src/Shared/Marketplace.Contracts/

# Restore dependencies
RUN dotnet restore src/Services/FilesService/FilesService.csproj

# Copy all source code
COPY . .

# Build and publish
WORKDIR /src/src/Services/FilesService
RUN dotnet publish FilesService.csproj -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
EXPOSE 80

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "FilesService.dll"]