# EnergyPlanner

Вебзастосунок для розрахунку енергоспоживання побутових пристроїв, часу автономної роботи акумуляторів і рекомендації потрібної ємності.

**Стек:** ASP.NET Core 8 Web API, EF Core 8, SQL Server, HTML/CSS/JS, xUnit, GitHub Actions, Docker.

## Запуск через Docker (Windows / Linux / macOS)

Потрібен Docker з Docker Compose.

```bash
git clone https://github.com/IvanKus13/EnergyPlannerProject.git
cd EnergyPlannerProject
cp .env.example .env
```

У `.env` вписати пароль для SQL Server. Він має бути не коротший за 8 символів і містити великі та малі літери, цифру й спецсимвол, наприклад:

```
MSSQL_SA_PASSWORD=Energy_Plan2026
```

Запуск:

```bash
docker compose up --build
```

Відкрити: **http://localhost:8080**

Compose піднімає три контейнери: SQL Server 2022, міграції бази даних і сам застосунок.

Зупинка: `docker compose down`. Щоб видалити ще й базу: `docker compose down -v`.

## Локальний запуск без Docker (Windows)

Потрібні .NET 8 SDK і SQL Server (або LocalDB).

1. За потреби змінити рядок підключення в `EnergyPlanner/appsettings.json`, наприклад `Server=(localdb)\MSSQLLocalDB;...`.
2. Застосувати міграції й запустити:

```bash
dotnet tool install --global dotnet-ef
cd EnergyPlanner
dotnet ef database update
dotnet run
```

Відкрити: **http://localhost:5118**. Swagger: `/swagger`.

## Тести

```bash
dotnet test
```

Покриття коду:

```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coveragereport -classfilters:-*Migrations*
```

Звіт відкривається з `coveragereport/index.html`.

## Статичний аналіз

Використовуються .NET Analyzers для C# і ESLint для JavaScript.

```bash
npm ci
npm run analyze
```

Звіти з'являються в папці `reports/`, зведений звіт - `static-analysis-report.pdf`.

## CI

GitHub Actions (`.github/workflows/ci.yml`) на кожен push і PR у `master` виконує restore, build і test.

## Команда

- Іван Кошеля 
- Ілля Ставицький 
- Артем Величко 
