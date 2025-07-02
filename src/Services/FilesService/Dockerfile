# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY ["FilesService.csproj", "."]
RUN dotnet restore "FilesService.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "FilesService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FilesService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FilesService.dll"]