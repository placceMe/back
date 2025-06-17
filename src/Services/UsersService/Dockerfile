# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копіюємо все
COPY . . 

# Відновлюємо залежності
RUN dotnet restore

# Публікуємо додаток у /publish
RUN dotnet publish -c Release -o /publish

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Копіюємо з попереднього етапу
COPY --from=build /publish .

# Відкриваємо порт
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Запускаємо сервіс
ENTRYPOINT ["dotnet", "UsersService.dll"]
