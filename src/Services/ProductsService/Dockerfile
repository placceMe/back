# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app

# Copy project file and restore with retry
COPY *.csproj ./
RUN dotnet tool install --global dotnet-ef && \
    dotnet nuget locals all --clear && \
    (dotnet restore --no-cache --disable-parallel || \
     (sleep 5 && dotnet restore --no-cache --disable-parallel) || \
     (sleep 10 && dotnet restore --no-cache --disable-parallel))

# Copy source code and publish
COPY . ./
RUN dotnet publish -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

COPY --from=build /out .

ENTRYPOINT ["dotnet", "ProductsService.dll"]
