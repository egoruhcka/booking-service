# 🏢 Room Booking Service

Сервис бронирования переговорок для тестового задания стажёра Backend (Avito).

Реализован на **ASP.NET Core Minimal API** с использованием **C#**, **PostgreSQL**, **Entity Framework Core** и **JWT-авторизации**.

---

## 🚀 Быстрый старт

### Вариант А: Локальный запуск (для разработки)

```bash
# 1. Запуск зависимостей (PostgreSQL)
docker compose up -d postgres

# 2. Применение миграций
dotnet ef database update

# 3. Запуск сервиса
dotnet run

# Сервис доступен на: http://localhost:8080
```

### Вариант Б: Запуск через Docker Compose (продакшен-подобный)

```bash
# Запуск всего стека (приложение + БД)
docker compose up --build

# Сервис доступен на: http://localhost:8080
# БД: localhost:5432 (user: app_user, pass: app_password, db: roombooking)
```

---

## 🔐 Аутентификация

### Тестовый режим (`/dummyLogin`)
Для быстрого тестирования бизнес-логики без регистрации:

```bash
# Получить токен админа
curl -X POST http://localhost:8080/dummyLogin \
  -H "Content-Type: application/json" \
  -d '{"role":"admin"}'

# Получить токен пользователя
curl -X POST http://localhost:8080/dummyLogin \
  -H "Content-Type: application/json" \
  -d '{"role":"user"}'
```

**Возвращает:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

> ⚠️ **Не использовать в продакшене!** Фиксированные UUID для ролей предназначены только для тестирования.

### Продакшен-режим (`/register`, `/login`) — ✅ БОНУС

```bash
# Регистрация нового пользователя
curl -X POST http://localhost:8080/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "securePass123",
    "role": "user"
  }'

# Вход по email/паролю
curl -X POST http://localhost:8080/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "securePass123"
  }'
```

**Особенности:**
- 🔐 Пароли хешируются через **BCrypt** (work factor = 12)
- ✉️ Валидация email и защита от дубликатов
- 🔒 Только роль `user` может зарегистрироваться самостоятельно
- 🎫 Возвращается JWT с `role` и `user_id` claims для авторизации

---

## 📡 Эндпоинты API

Все запросы (кроме `/dummyLogin`, `/register`, `/login`) требуют заголовок:
```
Authorization: Bearer <token>
```

### 🏢 Переговорки (`/rooms/*`)

| Метод | Эндпоинт | Описание | Роль |
|-------|----------|----------|------|
| `GET` | `/rooms/list` | Список всех переговорок | any |
| `POST` | `/rooms/create` | Создание переговорки | **admin** |

**Пример создания:**
```bash
curl -X POST http://localhost:8080/rooms/create \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name": "Board Room", "capacity": 10, "description": "For executives"}'
```

### 📅 Расписание (`/rooms/{id}/schedule/*`)

| Метод | Эндпоинт | Описание | Роль |
|-------|----------|----------|------|
| `POST` | `/rooms/{id}/schedule/create` | Создание расписания (только один раз!) | **admin** |

**Пример:**
```bash
curl -X POST http://localhost:8080/rooms/{roomId}/schedule/create \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "roomId": "uuid",
    "daysOfWeek": [1,2,3,4,5],
    "startTime": "09:00",
    "endTime": "18:00"
  }'
```

**Валидация:**
- `daysOfWeek`: массив чисел 1–7 (1=Пн, 7=Вс)
- `startTime`/`endTime`: формат `HH:MM`
- `endTime` должен быть позже `startTime`
- ⚠️ Повторное создание → `409 Conflict`

### 🎫 Слоты (`/rooms/{id}/slots/*`)

| Метод | Эндпоинт | Описание | Роль |
|-------|----------|----------|------|
| `GET` | `/rooms/{id}/slots/list?date=YYYY-MM-DD` | Список доступных 30-мин слотов на дату | any |

**Пример:**
```bash
curl "http://localhost:8080/rooms/{roomId}/slots/list?date=2026-04-07" \
  -H "Authorization: Bearer $TOKEN"
```

**Возвращает:**
```json
{
  "slots": [
    {
      "id": "det-uuid-based-on-room+date+time",
      "roomId": "uuid",
      "start": "2026-04-07T09:00:00.0000000Z",
      "end": "2026-04-07T09:30:00.0000000Z"
    }
  ]
}
```

### 📋 Бронирование (`/bookings/*`)

| Метод | Эндпоинт | Описание | Роль |
|-------|----------|----------|------|
| `POST` | `/bookings/create` | Создание брони | **user** |
| `GET` | `/bookings/my` | Мои будущие активные брони | **user** |
| `GET` | `/bookings/list?page=1&pageSize=20` | Все брони с пагинацией | **admin** |
| `POST` | `/bookings/{id}/cancel` | Отмена брони (идемпотентно) | **user** |

**Пример бронирования:**
```bash
curl -X POST http://localhost:8080/bookings/create \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"slotId": "slot-uuid"}'
```

**Пример отмены:**
```bash
curl -X POST http://localhost:8080/bookings/{bookingId}/cancel \
  -H "Authorization: Bearer $USER_TOKEN"
```

### 🔧 Системные эндпоинты

| Метод | Эндпоинт | Описание |
|-------|----------|----------|
| `GET` | `/` | Health check: `"Room Booking Service is running"` |
| `GET` | `/_info` | Health check: `200 OK` (для Kubernetes/Docker) |

---

## 🧠 Архитектурные решения

### 🎯 Гибридная генерация слотов (ключевое решение)

**Проблема:** Слоты должны иметь стабильные UUID для бронирования, но хранить все слоты на год вперёд (50 комнат × 48 слотов × 365 дней = ~876k записей) — избыточно.

**Решение:** Гибридный подход — **ленивая генерация + сохранение в БД**.

```
Запрос /slots?date=2026-04-07
         │
         ▼
┌─────────────────────┐
│ 1. Проверяем БД:    │
│    есть ли слоты    │
│    для этой даты?   │
└────────┬────────────┘
         │
    ┌────┴────┐
    ▼         ▼
 [Есть]    [Нет]
    │         │
    ▼         ▼
Возвращаем  Генерируем на основе
из БД       расписания:
            • Проверяем daysOfWeek
            • Создаём 30-мин интервалы
            • Генерируем UUID через
              MD5(roomId:date:startTime)
            • Сохраняем в БД
            • Возвращаем клиенту
```

**Преимущества:**
| Преимущество | Объяснение |
|-------------|-----------|
| ✅ Стабильность | Один и тот же слот всегда имеет одинаковый UUID → можно бронировать по `slotId` |
| ✅ Валидация | При бронировании проверяем существование слота через БД (не доверяем клиенту) |
| ✅ Экономия | Не храним слоты для дат, которые никто не запрашивал (99.9% запросов — ближайшие 7 дней) |
| ✅ Масштабируемость | 50 комнат × 48 слотов × 7 дней = ~16k записей вместо 876k |

**Обоснование для ТЗ:** ТЗ допускает два подхода: «скользящее окно» или «генерация при запросе». Выбран второй с сохранением в БД для корректной валидации бронирования.

### 🔐 Авторизация и роли

**Двухуровневая проверка:**
1.  `.RequireAuthorization()` — проверяет наличие валидного JWT (аутентификация)
2.  `context.IsAdmin()` / `context.IsUser()` — проверяет роль из claim'а (авторизация)

```csharp
// Пример проверки в эндпоинте
app.MapPost("/rooms/create", async (HttpContext context, ...) =>
{
    if (!context.IsAdmin())  // Универсальная проверка: работает и для dummy, и для реальных пользователей
        return Results.Forbid();
    
    // ... бизнес-логика
});
```

**Универсальность:** Проверка ролей через claim `role` работает одинаково для:
- Тестовых пользователей из `/dummyLogin` (фиксированные UUID)
- Реальных пользователей из `/register` (динамические UUID)

### 🗄️ Миграции и деплой

**Авто-применение миграций при старте:**
```csharp
// Program.cs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.GetPendingMigrations().Any())
    {
        app.Logger.LogInformation("Applying pending migrations...");
        db.Database.Migrate();
    }
}
```

**Зачем:** Гарантирует, что при запуске в Docker/CI база данных будет актуальной без ручного вмешательства.

---

## 🧪 Тестирование

### Запуск тестов

```bash
# Все тесты
dotnet test

# С отчётом о покрытии
dotnet test --collect:"XPlat Code Coverage"

# Отчёт будет в: TestResults/**/coverage.cobertura.xml
```

### Покрытие кода

Проект покрыт юнит-тестами (>40%):
- `TokenServiceTests` — генерация JWT, фиксированные UUID
- `PasswordHasherTests` — хеширование и верификация паролей (BCrypt)
- `SlotGeneratorTests` — детерминированная генерация UUID слотов
- `ModelTests` — инициализация сущностей

### Интеграционное тестирование

Все эндпоинты протестированы вручную через `curl` (см. раздел «Быстрый старт»). Для CI настроен GitHub Actions с запуском e2e-тестов в Docker.

---

## 🛠️ Стек технологий

| Компонент | Технология |
|-----------|-----------|
| **Язык** | C# 12 (.NET 9) |
| **Фреймворк** | ASP.NET Core Minimal API |
| **База данных** | PostgreSQL 15 + EF Core 9 |
| **Авторизация** | JWT (System.IdentityModel.Tokens.Jwt) |
| **Хеширование паролей** | BCrypt.Net (work factor = 12) |
| **Тесты** | xUnit 2.9 + coverlet |
| **Контейнеризация** | Docker + Docker Compose |
| **CI/CD** | GitHub Actions |

---

## 📁 Структура проекта

```
RoomBookingService/
├── Program.cs                    # Точка входа, регистрация сервисов
├── appsettings.json              # Базовая конфигурация
├── appsettings.Development.json  # Конфигурация для разработки
├── docker-compose.yml            # Оркестрация (app + postgres)
├── Dockerfile                    # Мульти-стейдж сборка
├── .gitignore                    # Исключения для git
├── README.md                     # Этот файл
│
├── Data/
│   ├── AppDbContext.cs           # EF Core контекст
│   ├── Migrations/               # Миграции БД
│   └── Models/                   # Сущности (User, Room, Schedule, Slot, Booking)
│
├── Endpoints/                    # Minimal API эндпоинты
│   ├── AuthEndpoints.cs          # /dummyLogin, /register, /login
│   ├── RoomEndpoints.cs          # /rooms/*
│   ├── ScheduleEndpoints.cs      # /rooms/{id}/schedule/*
│   ├── SlotEndpoints.cs          # /rooms/{id}/slots/*
│   └── BookingEndpoints.cs       # /bookings/*
│
├── Services/                     # Бизнес-логика
│   ├── TokenService.cs           # Генерация/валидация JWT
│   ├── PasswordHasher.cs         # Хеширование паролей (BCrypt)
│   └── SlotGenerator.cs          # Гибридная генерация слотов
│
├── Models/DTOs/                  # DTO для запросов/ответов
│   ├── AuthDto.cs
│   ├── RoomDto.cs
│   ├── ScheduleDto.cs
│   └── BookingDto.cs
│
├── Middleware/                   # Middleware-компоненты
│   └── RoleChecker.cs            # Extension-методы для проверки ролей
│
└── Tests/                        # Тесты
    ├── Unit/                     # Юнит-тесты
    │   ├── TokenServiceTests.cs
    │   ├── PasswordHasherTests.cs
    │   ├── SlotGeneratorTests.cs
    │   └── ModelTests.cs
    └── Integration/              # Интеграционные тесты (опционально)
```

---

## 🔧 Конфигурация

### Переменные окружения

| Переменная | Описание | Пример |
|-----------|----------|--------|
| `ConnectionStrings__Default` | Строка подключения к БД | `Host=localhost;Port=5432;Database=roombooking;Username=app_user;Password=app_password;SslMode=Disable` |
| `Jwt__Key` | Секретный ключ для JWT (мин. 32 символа) | `super-secret-key-for-test-task-min-32-chars!!` |
| `Jwt__Issuer` | Issuer claim в JWT | `roombooking-service` |
| `Jwt__Audience` | Audience claim в JWT | `roombooking-clients` |
| `ASPNETCORE_URLS` | Адрес прослушивания приложения | `http://0.0.0.0:8080` |

### Файлы конфигурации

- `appsettings.json` — базовые настройки (логирование, разрешённые хосты)
- `appsettings.Development.json` — переопределения для разработки (строка подключения к БД, JWT)

---

## 🐳 Docker

### Сборка образа

```bash
docker build -t roombooking-service .
```

### Запуск через Compose

```bash
# Запуск
docker compose up --build

# Остановка с удалением томов
docker compose down -v

# Просмотр логов
docker compose logs -f app
```

### Healthchecks

- **PostgreSQL**: `pg_isready -U app_user -d roombooking`
- **Приложение**: `wget --spider http://localhost:8080/_info`

---

## 📝 Лицензия

Тестовое задание для стажёра Backend (Avito). Код предоставлен в образовательных целях.