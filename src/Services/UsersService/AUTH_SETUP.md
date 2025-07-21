# Додавання JWT залежностей для UsersService

## NuGet пакети які потрібно додати:

```bash
cd p:\Norsen\back\src\Services\UsersService
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package BCrypt.Net-Next
```

## Або додати в UsersService.csproj:

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.0.0" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
```

## Після додавання пакетів потрібно оновити:

1. **AuthService.cs** - додати реальні JWT методи та BCrypt хешування
2. **Program.cs** - додати JWT конфігурацію
3. **appsettings.json** - додати JWT налаштування

## Готова базова структура авторизації:

- ✅ DTOs для авторизації
- ✅ Інтерфейси та сервіси
- ✅ Контролер з cookie-based авторизацією
- ✅ Базова реалізація без JWT (для тестування)
- ✅ HTTP тести

## Наступні кроки:

1. Додати NuGet пакети
2. Оновити AuthService з реальним JWT
3. Додати JWT middleware до Program.cs
4. Тестувати повну авторизацію