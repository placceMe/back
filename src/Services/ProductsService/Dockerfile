# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app

# Копіюємо csproj і restore
COPY *.csproj ./
RUN dotnet restore

# Копіюємо решту коду
COPY . ./
RUN dotnet publish -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "ProductsService.dll"]
