# ContentService

ContentService - це мікросервіс для управління контентом сайту в Marketplace додатку. Адміністратори зможуть змінювати контент сайту через цей сервіс.

## Опис

Цей сервіс дозволяє адміністраторам управляти статичним контентом веб-сайту, включаючи:
- Тексти сторінок
- HTML контент  
- Конфігураційні повідомлення
- Статичні дані для фронтенду

## Технологічний стек

- **.NET 8.0** - основна платформа
- **Entity Framework Core** - ORM для роботи з базою даних
- **PostgreSQL** - база даних (схема: content_service)
- **Serilog** - логування
- **Swagger** - документація API

## Порти

- **Development**: 5006
- **Docker**: 80 (мапиться на 5006)

## Endpoints

### Health Check
- `GET /api/content/health` - перевірка стану сервісу

## Конфігурація

### Environment Variables
- `ASPNETCORE_ENVIRONMENT` - середовище (Development/Production)
- `ConnectionStrings__DefaultConnection` - рядок підключення до PostgreSQL
- `ALLOWED_ORIGINS` - дозволені CORS origins

### База даних
Сервіс використовує PostgreSQL схему `content_service`.

## Розробка

### Локальний запуск
```bash
cd src/Services/ContentService
dotnet run
```

### Docker
```bash
docker-compose up content-service
```

## Статус
🚧 **В розробці** - Базова структура створена, моделі та бізнес-логіка будуть додані пізніше.

## Контакти
Цей сервіс є частиною Marketplace додатку.
