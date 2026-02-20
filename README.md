# AutoPulse

**Персональный мониторинг автомобилей по всему миру**

[![.NET](https://img.shields.io/badge/.NET-11-purple?logo=dotnet)](https://dotnet.microsoft.com)
[![Angular](https://img.shields.io/badge/Angular-21+-red?logo=angular)](https://angular.io)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue?logo=postgresql)](https://www.postgresql.org)
[![Redis](https://img.shields.io/badge/Redis-7-red?logo=redis)](https://redis.io)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.12-orange?logo=rabbitmq)](https://www.rabbitmq.com)
[![Playwright](https://img.shields.io/badge/Playwright-1.50-green?logo=playwright)](https://playwright.dev)

---

## О проекте

**AutoPulse** — это умная платформа для персонального мониторинга рынка автомобилей с глобальным охватом.

### Концепция

> **Выбери свою идеальную машину — мы найдём её по всему миру**

Пользователь выбирает марку, модель и параметры → система мониторит все рынки → показывает лучшие предложения в реальном времени.

### Ключевые возможности

- **Персональный поиск** — настрой параметры под себя (марка, модель, год, бюджет)
- **Глобальный мониторинг** — Китай, Европа, США, Япония
- **Умная очередь парсинга** — дедупликация запросов, приоритеты, расписание
- **Общие результаты** — один парсинг для всех пользователей
- **Real-time уведомления** — новые авто, снижение цен
- **Аналитика рынка** — динамика цен, статистика по поколениям
- **JWT авторизация** — регистрация, профиль, избранное

---

## Архитектура

```
┌─────────────────────────────────────────────────────────────┐
│                    Angular Dashboard                        │
│              (Auth, Search, Dashboard, Alerts)              │
└─────────────────────────┬───────────────────────────────────┘
                          │ HTTP/REST + JWT
┌─────────────────────────▼───────────────────────────────────┐
│                  AutoPulse.Api (.NET 11)                    │
│                  Minimal API + Endpoints                    │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  /api/auth/*       - регистрация, логин, профиль      │  │
│  │  /api/user/search/* - управление поиском              │  │
│  │  /api/cars/*       - список автомобилей               │  │
│  └───────────────────────────────────────────────────────┘  │
│         │                    │                    │          │
│         ▼                    ▼                    ▼          │
│   PostgreSQL            Redis              SignalR          │
│   (пользователи,         (кэш)              (real-time)     │
│    поиск, очередь)                                           │
└─────────────────────────────────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────┐
│               AutoPulse.Worker (.NET 11)                    │
│              Умная очередь парсинга                         │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  CarSearchQueueWorker                                │   │
│  │  ┌────────────────────────────────────────────────┐  │   │
│  │  │  Che168PlaywrightParser (Китай)               │  │   │
│  │  │  MobileDeParser (Европа) - в разработке       │  │   │
│  │  │  CarsComParser (США) - в разработке           │  │   │
│  │  └────────────────────────────────────────────────┘  │   │
│  │  Приоритеты: 10+ пользователей → каждые 15 мин       │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### Структура решения

| Проект | Описание |
|--------|----------|
| **AutoPulse.Api** | Minimal API, JWT auth, user search endpoints |
| **AutoPulse.Worker** | Умная очередь парсинга, фоновая обработка |
| **AutoPulse.Domain** | User, UserSearch, CarSearchQueue, PriceHistory |
| **AutoPulse.Application** | Бизнес-логика, CQRS (в разработке) |
| **AutoPulse.Infrastructure** | EF Core, Playwright, Redis, RabbitMQ |
| **AutoPulse.Parsing** | Che168PlaywrightParser (Китай), MobileDeParser (Европа), CarsComParser (США) |

---

## Технологический стек

### Backend
- **.NET 11** — основная платформа
- **Minimal API** — современные endpoints
- **Entity Framework Core** — ORM
- **PostgreSQL** — основная БД
- **Redis** — кэширование результатов
- **RabbitMQ** — асинхронные задачи (опционально)
- **SignalR** — real-time уведомления
- **Serilog** — структурированное логирование
- **JWT** — аутентификация

### Парсинг
- **Playwright 1.50** — headless браузер
- **Mobile User-Agent** — доступ к мобильным версиям сайтов
- **Умный скроллинг** — загрузка динамического контента

### Frontend (в разработке)
- **Angular 21** — фреймворк
- **Signals** — реактивное состояние
- **Bootstrap 5** — UI компоненты
- **RxJS** — реактивные потоки

### DevOps
- **GitHub Actions** — CI/CD

---

## Быстрый старт

### Требования

- [.NET 11 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 16+](https://www.postgresql.org/download/)
- [Node.js 20+](https://nodejs.org) (для фронтенда)
- [Playwright](https://playwright.dev) (устанавливается автоматически)

### 1. Настройка PostgreSQL

Создайте базу данных и обновите строку подключения в `appsettings.json`:

```bash
# Создать базу данных (выполнить в psql)
createdb -U postgres autopulse
```

Отредактируйте `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=autopulse;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### 2. Применение миграций

```bash
cd src/AutoPulse.Infrastructure
dotnet ef database update
```

### 3. Запуск API

```bash
cd src/AutoPulse.Api
dotnet run
```

API доступен по адресу: `https://localhost:7001` / `http://localhost:5001`

Swagger: `https://localhost:7001/swagger`

### 4. Запуск Worker (парсинг)

```bash
cd src/AutoPulse.Worker
dotnet run
```

Worker автоматически обрабатывает очередь парсинга.

---

## API Endpoints

### Авторизация

| Метод | Endpoint | Описание |
|-------|----------|----------|
| POST | `/api/auth/register` | Регистрация нового пользователя |
| POST | `/api/auth/login` | Вход (получение JWT токена) |
| GET | `/api/auth/me` | Получить текущий профиль |
| PUT | `/api/auth/me` | Обновить профиль |

### Поиск автомобилей

| Метод | Endpoint | Описание |
|-------|----------|----------|
| GET | `/api/user/search` | Получить текущий поиск |
| POST | `/api/user/search` | Сохранить поиск + запустить парсинг |
| DELETE | `/api/user/search` | Удалить поиск |
| GET | `/api/user/search/status` | Статус парсинга |

### Автомобили

| Метод | Endpoint | Описание |
|-------|----------|----------|
| GET | `/api/cars` | Список всех авто (пагинация) |
| GET | `/api/cars/{id}` | Детали автомобиля |
| GET | `/api/brands` | Список марок |
| GET | `/api/models` | Список моделей |

---

## Источники данных

| Регион | Источник | Статус | URL |
|--------|----------|--------|-----|
| Китай | Che168 (мобильная) | ✅ Готово | m.che168.com |
| Европа | Mobile.de | ✅ Готово | www.mobile.de |
| США | Cars.com | ✅ Готово | www.cars.com |
| Япония | Goo-net | В плане | www.goo-net.com |

---

## Умная очередь парсинга

### Как это работает:

```
Пользователь 1: Audi A3 (2015-2020)
    ↓
Проверка очереди: Есть ли уже такой запрос?
    ├─ НЕТ → Создать CarSearchQueue → Парсинг → Результаты в БД
    └─ ДА → Подписать пользователя на существующие данные
    ↓
Пользователь 2: Audi A3 (2016-2019)
    ↓
Проверка очереди: Уже есть Audi A3!
    └─ ДА → Просто добавить в список ожидания → Priority++
    ↓
Результат: Один парсинг обслуживает обоих пользователей!
```

### Приоритеты парсинга:

| Пользователей | Интервал парсинга |
|---------------|-------------------|
| 10+ | Каждые 15 мин |
| 5-9 | Каждые 30 мин |
| 2-4 | Каждый 1 час |
| 1 | Каждые 2 часа |

---

## Roadmap

### Q1 2026 ✅
- [x] Базовая структура решения
- [x] Умная очередь парсинга (UserSearch → CarSearchQueue)
- [x] JWT авторизация и профиль
- [x] API endpoints (/api/auth, /api/user/search)
- [x] Che168PlaywrightParser (мобильная версия)
- [ ] Миграции БД
- [ ] Worker для обработки очереди

### Q2 2026
- [ ] Парсер Европы (Mobile.de)
- [ ] Парсер США (Cars.com)
- [ ] Angular дашборд (Auth, Search, Cars)
- [ ] Real-time уведомления (SignalR)
- [ ] Аналитика рынка

### Q3 2026
- [ ] Telegram бот для уведомлений
- [ ] Email рассылки
- [ ] Избранное и сравнение авто
- [ ] ML-прогнозы цен

---

## Вклад

Проект находится в активной разработке. Если хочешь помочь:

1. Форкни репозиторий
2. Создай ветку (`git checkout -b feature/amazing-feature`)
3. Закоммить изменения (`git commit -m 'Add amazing feature'`)
4. Запуш (`git push origin feature/amazing-feature`)
5. Открой Pull Request

---

## Лицензия

MIT License — см. файл [LICENSE](LICENSE)

---

## Автор

**mrDeminAA**

GitHub: [@mrDeminAA](https://github.com/mrDeminAA)

---

<div align="center">

**Made with .NET 11, Angular 21 & Playwright**

Поставь звезду, если проект интересен!

[![Concept](https://img.shields.io/badge/Concept-Audi%20A3%20Tracker-blue)](A3_TRACKER_CONCEPT.md)

</div>
