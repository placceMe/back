# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy project file first for better layer caching
COPY ["UsersService.csproj", "."]

# Install EF Core tools globally and restore packages
RUN dotnet tool install --global dotnet-ef && \
    dotnet nuget locals all --clear && \
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

# Copy .NET tools from build stage
COPY --from=build /root/.dotnet/tools /home/appuser/.dotnet/tools
RUN chown -R appuser:appuser /home/appuser/.dotnet

USER appuser

# Add .NET tools to PATH for non-root user
ENV PATH="$PATH:/home/appuser/.dotnet/tools"

# Copy published app
COPY --from=build /app/publish .

# Copy entrypoint script
COPY entrypoint.sh ./
RUN chmod +x entrypoint.sh

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

EXPOSE 80

ENTRYPOINT ["./entrypoint.sh"]
