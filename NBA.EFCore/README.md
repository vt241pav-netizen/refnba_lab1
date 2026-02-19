## 1. Назва проєкту та опис предметної області 
###   Назва проєкту
**NBAEf** - система управління базою даних Національної Баскетбольної Асоціації (NBA)
###   Опис системи
Основні функції системи:
- Керування гравцями
- Оптимізований пошук
- Безпечне видалення
- Аналітика
###  Сутності бази даних
- **ARENAS** - баскетбольні арени
- **CONFERENCES** - конференції 
- **DIVISIONS** - дивізіони
- **TEAMS** - команди
- **PLAYERS** - гравці
- **COACHES** - тренери
- **MATCHES** - матчі
- **STATISTICS** - статистика гравців
## 2. Інструкції по налаштуванню

### Встановлення SQL Server
1. Завантажте **SQL Server 2022 Express**
2. Встановіть з налаштуваннями за замовчуванням
3. Запустіть **SQL Server Management Studio (SSMS)**
4. Створіть базу даних 

### Налаштування Connection String через User Secrets
bash
# Ініціалізація User Secrets
dotnet user-secrets init

# Додавання connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=database_nba;Trusted_Connection=true;TrustServerCertificate=true;"

# NuGet пакети
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design

# Згенерувати EFModels
dotnet ef dbcontext scaffold "Server=.;Database=database_nba;Trusted_Connection=True;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer --output-dir EFModels --context NbaDbContext --force --data-annotations

## 3. Інструкції по запуску
# Виконання міграцій
# Міграції виконуються автоматично при запуску програми
bash
dotnet run
# Запуск демонстрації
# Збирання та запуск проєкту
dotnet build
dotnet run
# Команди для використання
# Очищення та перебудова
dotnet clean
dotnet build
# Запуск без збирання
dotnet run --no-build
# Перегляд User Secrets
dotnet user-secrets list
## 4. Виконані завдання
# Завдання 1: Database Scaffolding 
# Завдання 2: DbContext та CRUD 
# Завдання 3: Складні LINQ запити 
# Завдання 4: Транзакції 
# Завдання 5 А
# Завдання 6 А 