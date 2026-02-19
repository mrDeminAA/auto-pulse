# AutoPulse - План разработки

## Спринт 1: База + Авторизация + Умная очередь (ВЫПОЛНЕН)

### Что сделано:

**БД (Domain + Infrastructure):**
- User - пользователи системы
- UserPreferences - предпочтения пользователей
- UserCar - избранное
- CarAlert - уведомления
- PriceHistory - история цен
- UserSearch - поисковые запросы пользователей
- CarSearchQueue - умная очередь парсинга
- UserSearchQueue - связь поиска и очереди
- EF Core конфигурация всех сущностей

**API (Auth + UserSearch):**
- `/api/auth/register` - регистрация
- `/api/auth/login` - логин
- `/api/auth/me` (GET/PUT) - профиль
- `/api/user/search` (GET/POST/DELETE) - управление поиском
- `/api/user/search/status` - статус парсинга
- JWT аутентификация
- Password hashing (PBKDF2)

**Parsing:**
- Che168PlaywrightParser - мобильная версия m.che168.com
- Playwright браузер с iPhone User-Agent
- Скроллинг для загрузки контента
- Получение и сопоставление изображений

**Документация:**
- A3_TRACKER_CONCEPT.md - концепция системы
- Che168PlaywrightParser.README.md

---

## Спринт 2: Worker + Парсинг (В ПРОЦЕССЕ)

### Задачи:

**Worker (умная очередь):**
- [ ] CarSearchQueueWorker - фоновый сервис
- [ ] Приоритеты в очереди (по количеству пользователей)
- [ ] Расписание парсинга (15 мин - 2 часа)
- [ ] Блокировки (чтобы 2 воркера не парсили одно)
- [ ] Обработка ошибок и retry

**Parsing (адаптация):**
- [ ] Адаптировать Che168PlaywrightParser под очередь
- [ ] Конвертация валют (CNY → RUB)
- [ ] Дедупликация автомобилей (по URL + VIN)
- [ ] Сохранение в Cars (общие для всех)

**Миграции БД:**
- [ ] Создать миграцию `AddUserMonitoringEntities`
- [ ] Применить миграции

---

## Спринт 3: Frontend (Angular)

### Задачи:

**Auth компоненты:**
- [ ] LoginComponent - страница входа
- [ ] RegisterComponent - страница регистрации
- [ ] AuthService - JWT токены, interceptor
- [ ] AuthGuard - защита роутов

**Car Search компоненты:**
- [ ] CarSearchComponent - выбор машины
- [ ] Brand/Model selectors
- [ ] Параметры поиска (год, цена, пробег)
- [ ] Кнопка "Старт"

**Dashboard:**
- [ ] DashboardComponent - список авто
- [ ] CarCardComponent - карточка автомобиля
- [ ] FiltersComponent - фильтры
- [ ] SearchStatusComponent - статус парсинга

**Сервисы:**
- [ ] CarsService - API к автомобилям
- [ ] UserSearchService - API к поиску
- [ ] SignalRService - real-time уведомления

---

## Спринт 4: Уведомления + Аналитика

### Задачи:

**Уведомления:**
- [ ] Telegram Bot
- [ ] Email уведомления
- [ ] SignalR для real-time
- [ ] Логика триггеров (новая машина, снижение цены)

**Аналитика:**
- [ ] Средняя цена по рынку
- [ ] Динамика цен
- [ ] Статистика по поколениям
- [ ] Графики и диаграммы

---

## Следующие шаги

1. **Создать Worker** для обработки очереди
2. **Применить миграции БД**
3. **Протестировать API** (Postman/Swagger)
4. **Создать Frontend** компоненты

---

## Технологический стек

| Компонент | Технология |
|-----------|------------|
| Backend | ASP.NET Core 11 |
| Frontend | Angular 21 |
| БД | PostgreSQL |
| Кэш | Redis |
| Очередь | RabbitMQ |
| Парсинг | Playwright |
| Auth | JWT |

---

**Статус:** Спринт 1 | Спринт 2 | Спринт 3 | Спринт 4
