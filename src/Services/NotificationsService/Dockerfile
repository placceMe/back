FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy solution file if exists (optional)
COPY *.sln ./

# Copy project files for better layer caching
COPY src/Services/NotificationsService/NotificationsService.csproj src/Services/NotificationsService/
COPY src/Shared/Marketplace.Contracts/Marketplace.Contracts.csproj src/Shared/Marketplace.Contracts/

# Restore dependencies
RUN dotnet restore src/Services/NotificationsService/NotificationsService.csproj

# Copy all source code
COPY . .

# Build and publish
WORKDIR /src/src/Services/NotificationsService
RUN dotnet publish NotificationsService.csproj -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "NotificationsService.dll"]
