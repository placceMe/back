# build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy solution file if exists (optional)
COPY *.sln ./

# Copy project files for better layer caching
COPY src/ApiGateway/ApiGateway.csproj src/ApiGateway/
COPY src/Shared/Marketplace.Contracts/Marketplace.Contracts.csproj src/Shared/Marketplace.Contracts/

# Restore dependencies
RUN dotnet restore src/ApiGateway/ApiGateway.csproj

# Copy all source code
COPY . .

# Build and publish
WORKDIR /src/src/ApiGateway
RUN dotnet publish ApiGateway.csproj -c Release -o /app/out

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
EXPOSE 80

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "ApiGateway.dll"]
