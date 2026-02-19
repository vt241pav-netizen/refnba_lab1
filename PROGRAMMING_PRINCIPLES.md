# Принципи програмування в проєкті NBA.EFCore

## 1. Принцип єдиної відповідальності (Single Responsibility Principle)
Класи в проєкті розділені за відповідальністю:
- **Репозиторії** (`TeamRepository`, `PlayerRepository` тощо) відповідають лише за доступ до даних.
- **Сервіси** (`AuthService`, `NbaService`) містять бізнес-логіку (автентифікація, складні запити, транзакції).
- **Програма** (`Program.cs`) відповідає лише за взаємодію з користувачем (меню, введення/виведення).

**Приклад:  **
Клас `AuthService` виконує лише логіку аутентифікації – методи `AuthenticateAsync` та `InitializeTestUsersAsync`.  
[Посилання на рядки](NBA.EFCore/Program.cs#L159-L170).

## 2. Принцип інверсії залежностей (Dependency Inversion Principle)
Модулі верхнього рівня (наприклад, `Program`) не залежать від конкретних реалізацій, а від абстракцій (інтерфейсів). Усі залежності передаються через конструктори.

**Приклад:  **
У `Program` оголошені поля типів інтерфейсів:
- private static ITeamRepository _teamRepository = null!;
- private static IAuthService _authService = null!;
- [Посилання на рядки](NBA.EFCore/Program.cs#L14-L22)
