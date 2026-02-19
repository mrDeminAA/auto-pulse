# AutoPulse — Быстрый старт (без Docker)

## Требования

- .NET 11 SDK
- PostgreSQL 16+
- Node.js 20+ (для фронтенда)

---

## 1. Настройка PostgreSQL

### Вариант A: Через pgAdmin
1. Откройте pgAdmin
2. Создайте базу данных `autopulse`
3. Запомните пароль пользователя `postgres`

### Вариант B: Через командную строку
```bash
createdb -U postgres autopulse
```

---

## 2. Настройка подключения

Откройте файл `appsettings.json` и укажите ваш пароль:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=autopulse;Username=postgres;Password=ВАШ_ПАРОЛЬ"
  }
}
```

> **Важно:** Redis и RabbitMQ отключены по умолчанию. Если они установлены — раскомментируйте строки.

---

## 3. Применение миграций

```bash
cd src/AutoPulse.Infrastructure
dotnet ef database update
```

Если миграции не применяются:
```bash
# Удалить все миграции
dotnet ef migrations remove

# Создать новую миграцию
dotnet ef migrations add InitialCreate

# Применить
dotnet ef database update
```

---

## 4. Запуск API

```bash
cd src/AutoPulse.Api
dotnet run
```

Проверка:
- Swagger UI: https://localhost:7001/swagger
- Health check: https://localhost:7001/health

---

## 5. Запуск Worker (парсинг)

Откройте **новый терминал**:

```bash
cd src/AutoPulse.Worker
dotnet run
```

Worker автоматически обрабатывает очередь парсинга.

---

## 6. Установка Playwright

При первом запуске Worker установит браузеры автоматически.

Или вручную:
```bash
pwsh bin/Debug/net11.0/playwright.ps1 install
```

---

## Проверка работоспособности

| Компонент | Как проверить |
|-----------|---------------|
| PostgreSQL | `psql -U postgres -d autopulse` |
| API | Открыть https://localhost:7001/swagger |
| Worker | Логи в консоли, таблица `CarSearchQueue` в БД |

---

## Решение проблем

### Ошибка подключения к БД
```
Failed to connect to PostgreSQL
```
Проверьте:
- PostgreSQL запущен (служба `postgresql-x64-16`)
- Пароль в `appsettings.json` верный
- БД `autopulse` существует

### Ошибка миграций
```
The database is not empty
```
Удалите БД и создайте заново:
```bash
dropdb -U postgres autopulse
createdb -U postgres autopulse
dotnet ef database update
```

### Worker не парсит
```
No search requests in queue
```
Это нормально. Сначала создайте поисковый запрос через API:
```bash
POST https://localhost:7001/api/user/search
```

---

## Следующие шаги

1. Зарегистрировать пользователя: `POST /api/auth/register`
2. Войти: `POST /api/auth/login`
3. Создать поисковый запрос: `POST /api/user/search`
4. Worker автоматически запустит парсинг

---

**Готово!**
