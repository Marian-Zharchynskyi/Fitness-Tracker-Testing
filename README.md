# Fitness-Tracker API

Додаток для відстеження фітнесу з тренуваннями, вправами та моніторингом прогресу.

## Технологічний стек
- **Framework**: .NET 8, ASP.NET Core Web API
- **ORM**: Entity Framework Core
- **СУБД**: PostgreSQL 16
- **Тестування**: xUnit, FluentAssertions, Moq, Testcontainers, WebApplicationFactory, k6

## Передумови для запуску
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (для Testcontainers та локальної БД)
- [k6](https://k6.io/docs/get-started/installation/) (для тестів продуктивності)

## Запуск проекту (локально)

1. Запустіть PostgreSQL через Docker:
   ```bash
   docker run --name fitness-db -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=fitness-tracker-db -p 5432:5432 -d postgres:16
   ```
2. Запустіть міграції бази даних:
   ```bash
   dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/API/API.csproj
   ```
3. Запустіть API:
   ```bash
   dotnet run --project src/API/API.csproj
   ```

Swagger UI буде доступний за адресою `http://localhost:5146/swagger`. При старті автоматично створюється база даних та застосовуються міграції.

## Тестування

Для запуску всіх тестів (Unit, Integration, Database):
```bash
dotnet test --verbosity normal
```

### Генерація звіту покриття коду
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
reportgenerator -reports:./coverage/**/coverage.cobertura.xml -targetdir:./coverage/report -reporttypes:"Html;TextSummary" -classfilters:"-Tests.*"
```
Відкрийте `coverage/report/index.html` у браузері.

### Тести продуктивності (k6)
Тести знаходяться у директорії `tests/k6`.
```bash
# Smoke test (швидка перевірка)
k6 run tests/k6/smoke-test.js

# Load test статистики
k6 run tests/k6/stats-load-test.js

# Stress test тренувань
k6 run tests/k6/workout-stress-test.js
```
