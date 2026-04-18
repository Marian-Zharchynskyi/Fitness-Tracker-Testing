# CI/CD з GitHub Actions для Fitness-Tracker (Lab 8)

## 1. Завдання 1: Workflow перевірки імені гілки
**Файл:** `.github/workflows/branch-name.yml`

**Обрана конвенція:** `^(feature|bugfix|hotfix|release)\/.+$` (наприклад, `feature/add-login`, `bugfix/fix-stats`).
**Обґрунтування:** Ця конвенція базується на популярній моделі розгалуження Git Flow. Вона чітко ідентифікує тип роботи (нова фіча, виправлення помилки, термінове виправлення, реліз), що спрощує навігацію репозиторієм, автоматизацію релізів та код-рев'ю.

## 2. Завдання 2: Збірка та тестування з Testcontainers
**Файли:** 
- `.github/workflows/ci.yml` (запускається при push у `main`)
- `.github/workflows/pr-tests.yml` (запускається при pull request у `main`)

Ці workflow виконують:
1. Встановлення `.NET 8`
2. `dotnet restore`, `dotnet build`
3. `dotnet test` (включає unit, integration та db-тести, де testcontainers запускають Postgres).

**Налаштування Branch Protection:**
- Гілка `main` захищена правилами:
  - Require a pull request before merging
  - Require status checks to pass before merging (`build-and-test` job)
*(Тут має бути скріншот налаштованого Branch Protection)*

## 3. Завдання 3: Валідація міграцій EF Core
**Файл:** `.github/workflows/migration.yml`

При кожному pull request перевіряється, чи немає нестворених міграцій (за допомогою `has-pending-model-changes`).
Потім міграції застосовуються до Postgres (щоб перевірити коректність виконання) і генерується ідемпотентний SQL-скрипт (`migrations.sql`), який публікується як артефакт.
Це гарантує, що розробник не забув створити міграцію, якщо змінив модель, і що міграції можуть бути успішно розгорнуті на прод.

## 4. Завдання 4: Performance-тестування з k6
**Файл:** `.github/workflows/k6.yml`

**Файл тестів:** `tests/k6/smoke-test.js`

Цей workflow може запускатися:
- На `pull_request` (завжди виконує швидкий `smoke` тест).
- Через `workflow_dispatch` (можна обрати `smoke`, `stats-load`, `progress-load`, `workout-stress`).

**Обрані k6 SLO (Service Level Objectives) в smoke-test.js:**
- `http_req_duration: ['p(95)<200']` — 95% запитів повинні відпрацьовувати швидше ніж за 200 мс. Це базовий критерій доступності та швидкодії для простого health/readiness запиту.
- `http_req_failed: ['rate<0.01']` — Менше 1% запитів можуть завершитися з помилкою (HTTP статуси відмінні від 2xx-3xx).
- `errors: ['rate<0.01']` — Рівень бізнес-помилок (кастомна метрика k6) не повинен перевищувати 1%.

**Обґрунтування SLO:** Оскільки smoke-тест запускається на кожному PR, він має бути швидким (10 секунд) і надійним. Основна його мета - перевірити, чи стартує API без помилок під легким навантаженням. 200 мс для p(95) - реалістична вимога для локального розгортання в Actions без складної бізнес-логіки в readiness-маршруті.

*(Тут мають бути посилання на успішні запуски workflows (Actions UI))*
