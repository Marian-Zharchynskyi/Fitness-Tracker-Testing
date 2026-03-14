# Fitness-Tracker — Project Context
---

## Загальний опис

**Fitness-Tracker** — REST API-сервер на базі ASP.NET Core 8, побудований за принципами **Clean Architecture**. Проект є заготовкою (стартером), у якому наразі реалізована лише сутність **User** із базовим CRUD. У майбутньому планується розширення доменної моделі (тренування, вправи, прогрес тощо).

---

## Стек технологій

| Технологія | Версія | Призначення |
|---|---|---|
| .NET / ASP.NET Core | 8.0 | Фреймворк для API |
| Entity Framework Core | 8.0 | ORM для роботи з БД |
| PostgreSQL | latest | Реляційна база даних |
| Npgsql + EFCore.NamingConventions | 8.x | PostgreSQL адаптер + snake_case конвенції |
| MediatR | 12.x | CQRS (Command/Query Mediator pattern) |
| FluentValidation | 11.x | Валідація команд |
| Optional | 4.0 | Монада `Option<T>` замість null |
| LanguageExt.Core | 4.x | Функціональні типи (залишено для розширюваності) |
| xUnit | 2.6 | Тестовий фреймворк |
| Testcontainers.PostgreSql | 3.6 | PostgreSQL у Docker для інтеграційних тестів |
| FluentAssertions | 6.x | Зручні Assert-и у тестах |
| Swashbuckle / Swagger | 7.x | Документація API |

---

## Архітектура

Проект побудований за **Clean Architecture**:

```
+-----------------------------------------------------------+
|                         API                               |  <- HTTP Controllers, DTOs, Startup
+-----------------------------------------------------------+
|                     Application                           |  <- Commands, Queries, Interfaces, Validators
+-----------------------------------------------------------+
|                      Domain                               |  <- Entities, Value Objects
+-----------------------------------------------------------+
|                   Infrastructure                          |  <- EF Core, Repos, DB Config, Migrations
+-----------------------------------------------------------+
```

Залежності суворо спрямовані **всередину**: API -> Application -> Domain <- Infrastructure.

---

## Структура рішення

```
Fitness-Tracker/
├── Fitness-Tracker.sln
├── src/
│   ├── API/
│   │   ├── Controllers/UsersController.cs
│   │   ├── DTOs/Users/
│   │   │   ├── UserDto.cs
│   │   │   ├── CreateUserDto.cs
│   │   │   └── UpdateUserDto.cs
│   │   ├── Modules/Errors/
│   │   │   ├── UserErrorHandler.cs
│   │   │   └── AuthenticationErrorHandler.cs
│   │   ├── Modules/DbModule.cs
│   │   ├── Modules/SetupModule.cs
│   │   └── Program.cs
│   │
│   ├── Application/
│   │   ├── Common/Behaviours/ValidationBehaviour.cs
│   │   ├── Common/Interfaces/Repositories/IUserRepository.cs
│   │   ├── Common/Interfaces/Queries/IUserQueries.cs
│   │   ├── Common/Result.cs
│   │   ├── Users/Commands/
│   │   │   ├── CreateUserCommand.cs + Validator
│   │   │   ├── UpdateUserCommand.cs + Validator
│   │   │   └── DeleteUserCommand.cs + Validator
│   │   ├── Users/Exceptions/UserException.cs
│   │   └── ConfigureApplication.cs
│   │
│   ├── Domain/
│   │   └── Users/
│   │       ├── User.cs
│   │       └── UserId.cs
│   │
│   └── Infrastructure/
│       ├── Persistence/
│       │   ├── ApplicationDbContext.cs
│       │   ├── ApplicationDbContextInitialiser.cs
│       │   ├── ConfigurePersistence.cs
│       │   ├── Configurations/UserConfiguration.cs
│       │   ├── Converters/DateTimeUtcConverter.cs
│       │   └── Repositories/UserRepository.cs
│       ├── ConfigureInfrastructure.cs
│       └── ConfigureSwaggerAuth.cs
│
└── tests/
    ├── Api.Tests.Integration/
    │   └── Users/UsersControllerTests.cs
    ├── Tests.Common/
    │   ├── BaseIntegrationTest.cs
    │   ├── TestFactory.cs
    │   └── TestsExtensions.cs
    └── Tests.Data/
        └── UsersData.cs
```

---

## Доменна модель (поточна)

### User

| Поле | Тип | Обов'язкове | Опис |
|---|---|---|---|
| `Id` | `UserId` (Guid wrapper) | ✅ | Strongly-typed primary key |
| `Email` | `string` | ✅ | Email (max 255) |
| `Name` | `string?` | ❌ | Ім'я (max 50) |
| `Surname` | `string?` | ❌ | Прізвище (max 50) |
| `PhoneNumber` | `string?` | ❌ | Телефон (max 20) |
| `PasswordHash` | `string` | ✅ | Хеш паролю (max 255) |
| `ClerkId` | `string?` | ❌ | Зовнішній ID (зарезервовано для майб. Auth, max 100) |

**Фабричний метод:** `User.New(id, email, name, surname, phoneNumber, passwordHash, clerkId?)`

**Методи оновлення:**
- `UpdateUser(email, name, surname, phoneNumber)`
- `UpdatePassword(passwordHash)`

---

## Ключові архітектурні принципи

### 1. CQRS через MediatR
- **Commands** (Create, Update, Delete) → повертають `Result<User, UserException>`
- **Queries** (GetAll, GetById, SearchByEmail, GetPaged) → через `IUserQueries`
- `UserRepository` реалізує **обидва** інтерфейси: `IUserRepository` + `IUserQueries`

### 2. Result монада замість throw/catch у контролерах
```csharp
var result = await sender.Send(command, ct);
return result.Match<ActionResult<UserDto>>(
    user  => UserDto.FromDomainModel(user),
    error => error.ToObjectResult()   // UserException -> HTTP status
);
```

### 3. Option<T> замість null
```csharp
Task<Option<User>> GetById(UserId id, CancellationToken ct);
// використання:
entity.Match<ActionResult>(u => Ok(u), () => NotFound());
```

### 4. Strongly-typed IDs
```csharp
public record UserId(Guid Value);
// Передати звичайний Guid туди де очікується UserId — compile error
```

### 5. EF Core Fluent API конфігурація
Всі обмеження й конвертери описані в `IEntityTypeConfiguration<T>` класах.
snake_case naming convention додана через `EFCore.NamingConventions`.

### 6. MediatR Pipeline Validation
`ValidationBehaviour<TRequest, TResponse>` автоматично запускає FluentValidation-валідатори.

---

## Потік запиту

```
HTTP Request
  -> UsersController (DTO -> Command/Query)
  -> MediatR Pipeline
     -> ValidationBehaviour (FluentValidation)
     -> CommandHandler / QueryHandler
     -> UserRepository (EF Core)
     -> PostgreSQL
```

---

## Тестова інфраструктура

| Компонент | Роль |
|---|---|
| `IntegrationTestWebFactory` | Замінює БД на Testcontainers PostgreSQL |
| `BaseIntegrationTest` | Надає `Sender`, `Context`, `Client` |
| `TestAuthHandler` | Підмінює автентифікацію на тестову схему |
| `UsersData` | Тестові дані для сидування |

> **Вимога для тестів:** Docker має бути запущений.

---

## Що було видалено при міграції з OstrohProblems

| Видалено | Причина |
|---|---|
| RoleNames, RoleId, Role | Немає ролей у поточній моделі |
| IIdentityService, IImageService | Немає auth/image сервісів |
| IClerkApiService, IHashPasswordService | Немає Clerk/hash інтеграції |
| UserImage, UserImageDto | Немає зображень |
| UpdateUserVm (Domain.ViewModels) | Замінено на UpdateUserDto у API шарі |
| Тести Location / Organisation | Немає відповідних сутностей |
| AWSSDK.S3 | Немає файлового сховища |
| Microsoft.AspNetCore.Identity.\* | Немає ASP.NET Identity |
| Унікальний індекс на ClerkId | Nullable + неактивна інтеграція |

---

## Запуск

```bash
# API
dotnet run --project src/API

# Тести (Docker required)
dotnet test tests/Api.Tests.Integration
```

Swagger UI: `https://localhost:{port}/swagger`

Необхідний рядок підключення в `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=fitness_tracker;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

---

## Наступні кроки

- [ ] Автентифікація (JWT / ASP.NET Identity)
- [ ] Хешування паролів (BCrypt / PBKDF2)
- [ ] EF Core міграції
- [ ] Доменні сутності: Workout, Exercise, WorkoutSession, ProgressRecord
- [ ] Unit-тести для Command Handlers
