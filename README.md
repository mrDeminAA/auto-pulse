# 🚗 AutoPulse

**Аналитическая платформа для мониторинга зарубежного рынка автомобилей**

[![.NET](https://img.shields.io/badge/.NET-11-purple?logo=dotnet)](https://dotnet.microsoft.com)
[![Angular](https://img.shields.io/badge/Angular-19+-red?logo=angular)](https://angular.io)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue?logo=postgresql)](https://www.postgresql.org)
[![Redis](https://img.shields.io/badge/Redis-7-red?logo=redis)](https://redis.io)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.12-orange?logo=rabbitmq)](https://www.rabbitmq.com)

---

## 📖 О проекте

**AutoPulse** — это мощная аналитическая система для сбора, обработки и визуализации данных о зарубежном автомобильном рынке.

### 🔥 Возможности

- 📊 **Мониторинг рынка** — отслеживание цен, моделей, комплектаций
- 🌍 **Мультирегиональность** — США, Европа, Китай, Корея, Япония
- 🤖 **Автоматический парсинг** — сбор данных с популярных автопорталов
- 📈 **Аналитика и тренды** — динамика цен, популярность моделей, сравнение рынков
- 🔔 **Уведомления** — алерты при изменении цен или появлении новых авто
- 📉 **Дашборды** — интерактивные графики и отчёты

---

## 🏗️ Архитектура

```
┌─────────────────────────────────────────────────────────────┐
│                    Angular Dashboard                        │
│                   (NgRx + Highcharts)                       │
└─────────────────────────┬───────────────────────────────────┘
                          │ HTTP/REST
┌─────────────────────────▼───────────────────────────────────┐
│                  AutoPulse.Api (.NET 11)                    │
│                     Clean Architecture                      │
│  ┌───────────────────────────────────────────────────────┐  │
│  │           Background Services (MassTransit)           │  │
│  │  ┌──────────────┐         ┌────────────────────────┐  │  │
│  │  │ Parser Jobs  │         │  Analytics Processor   │  │  │
│  │  └──────────────┘         └────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────┘  │
│         │                    │                    │          │
│         ▼                    ▼                    ▼          │
│   PostgreSQL            Redis              RabbitMQ         │
│   (основные данные)     (кэш)              (очереди)        │
└─────────────────────────────────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────┐
│               AutoPulse.Worker (.NET 11)                    │
│              (тяжёлые задачи парсинга)                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  USA Parser  │  │ EU Parser    │  │ CN Parser    │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
```

### 📦 Структура решения

| Проект | Описание |
|--------|----------|
| **AutoPulse.Api** | HTTP API, контроллеры, endpoints |
| **AutoPulse.Worker** | Фоновые задачи, парсинг, обработка данных |
| **AutoPulse.Domain** | Доменные сущности, value objects |
| **AutoPulse.Application** | Бизнес-логика, use cases, CQRS handlers |
| **AutoPulse.Infrastructure** | EF Core, RabbitMQ, внешние сервисы |

---

## 🛠️ Технологический стек

### Backend
- **.NET 11** — основная платформа
- **Clean Architecture** — чистая архитектура
- **CQRS** — разделение команд и запросов
- **Entity Framework Core** — ORM
- **MassTransit** — работа с очередями
- **PostgreSQL** — основная БД
- **Redis** — кэширование
- **RabbitMQ** — асинхронная обработка
- **Seq** — централизованное логирование

### Frontend (в разработке)
- **Angular 19+** — фреймворк
- **NgRx SignalStore** — управление состоянием
- **Highcharts / Apache ECharts** — визуализация данных
- **Bootstrap / Material** — UI компоненты

### DevOps
- **Docker** — контейнеризация
- **Docker Compose** — оркестрация локально
- **GitHub Actions** — CI/CD (планируется)

---

## 🚀 Быстрый старт

### Требования

- [.NET 11 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Node.js 20+](https://nodejs.org) (для фронтенда)

### 1. Запуск инфраструктуры

```bash
docker-compose up -d
```

Запустятся:
- PostgreSQL (порт 5432)
- Redis (порт 6379)
- RabbitMQ (порт 5672, UI: 15672)
- Seq (порт 5341)

### 2. Запуск API

```bash
cd src/AutoPulse.Api
dotnet run
```

API доступен по адресу: `https://localhost:7001` / `http://localhost:5001`

### 3. Запуск Worker

```bash
cd src/AutoPulse.Worker
dotnet run
```

### 4. Swagger

Открой `https://localhost:7001/swagger` для просмотра API документации.

---

## 📊 Источники данных

Система планирует парсить данные из:

| Регион | Источники |
|--------|-----------|
| 🇺🇸 США | Cars.com, AutoTrader, Edmunds |
| 🇪🇺 Европа | Mobile.de, AutoScout24 |
| 🇨🇳 Китай | Autohome, Dongchedi |
| 🇰🇷 Корея | Encar, Bobaedream |
| 🇯🇵 Япония | Goo-net, CarSensor |

---

## 📈 Roadmap

### Q1 2026
- [x] Базовая структура решения
- [ ] Настройка инфраструктуры (БД, очереди)
- [ ] Базовые сущности (Car, Dealer, Brand, Model)
- [ ] Парсер США (Cars.com)

### Q2 2026
- [ ] Парсер Европы и Азии
- [ ] Аналитический модуль
- [ ] Angular дашборд
- [ ] Система уведомлений

### Q3 2026
- [ ] Мобильное приложение
- [ ] ML-прогнозы цен
- [ ] Экспорт отчётов (PDF, Excel)

---

## 🤝 Вклад

Проект находится в активной разработке. Если хочешь помочь:

1. Форкни репозиторий
2. Создай ветку (`git checkout -b feature/amazing-feature`)
3. Закоммить изменения (`git commit -m 'Add amazing feature'`)
4. Запуш (`git push origin feature/amazing-feature`)
5. Открой Pull Request

---

## 📄 Лицензия

MIT License — см. файл [LICENSE](LICENSE)

---

## 👨‍💻 Автор

**mrDeminAA**

GitHub: [@mrDeminAA](https://github.com/mrDeminAA)

---

## 📬 Контакты

- Email: (укажи при желании)
- Telegram: (укажи при желании)

---

<div align="center">

**Made with ❤️ using .NET 11 & Angular**

⭐ Поставь звезду, если проект интересен!

</div>
