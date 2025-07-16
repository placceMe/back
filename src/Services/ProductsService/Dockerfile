# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app

# Копіюємо csproj і restore
COPY *.csproj ./
RUN dotnet tool install --global dotnet-ef && \
    dotnet restore

# Копіюємо решту коду
COPY . ./
RUN dotnet publish -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# Copy .NET tools from build stage
COPY --from=build /root/.dotnet/tools /root/.dotnet/tools
ENV PATH="$PATH:/root/.dotnet/tools"

COPY --from=build /out .
ENTRYPOINT ["dotnet", "ProductsService.dll"]
