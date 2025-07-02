# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy project file first for better layer caching
COPY ["UsersService.csproj", "."]

# Clear any existing NuGet config and restore
RUN dotnet nuget locals all --clear && \
    dotnet restore "UsersService.csproj" --no-cache

# Copy source code
COPY . .

# Build and publish (with fresh restore to avoid cache issues)
RUN dotnet publish "UsersService.csproj" -c Release -o /app/publish

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app

# Add non-root user for security
RUN adduser -D -s /bin/sh appuser
USER appuser

# Copy published app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

EXPOSE 80

ENTRYPOINT ["dotnet", "UsersService.dll"]
