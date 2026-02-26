# Принципи програмування в NBA Database Management System

## 1. SOLID Principles

### S - Single Responsibility Principle (Принцип єдиної відповідальності)

**Опис:** Кожен клас має лише одну причину для змін.

**Приклади в коді:**

- **[PlayerRepository.cs](https://github.com/vt241pav-netizen/refnba_lab1/blob/11165d737a1bd18b764b4c18eaa122eb620fbe83/NBA.EFCore/Repositories/PlayerRepository.cs#L11-L118)** - відповідає ТІЛЬКИ за роботу з гравцями (CRUD операції)
- **[TeamRepository.cs](https://github.com/vt241pav-netizen/refnba_lab1/blob/11165d737a1bd18b764b4c18eaa122eb620fbe83/NBA.EFCore/Repositories/TeamRepository.cs#L8-L144)** - відповідає ТІЛЬКИ за роботу з командами
- **[AuthService.cs](https://github.com/vt241pav-netizen/refnba_lab1/blob/11165d737a1bd18b764b4c18eaa122eb620fbe83/NBA.EFCore/Services/AuthService.cs#L10-L120)** - відповідає ТІЛЬКИ за аутентифікацію
- 
### O - Open/Closed Principle (Принцип відкритості/закритості)
Опис: Класи відкриті для розширення, але закриті для модифікації.

**Приклади в коді:**

IRepository інтерфейси - дозволяють додавати нові реалізації без змін існуючого коду
BaseEntity - можна розширювати новими сутностями




### L - Liskov Substitution Principle (Принцип підстановки Лісков)
Опис: Похідні класи можуть замінювати базові без порушення роботи.

Приклади в коді:

Всі репозиторії - можна замінити один одним

NbaService.cs - працює з будь-яким DbContext


### I - Interface Segregation Principle (Принцип розділення інтерфейсів)
Опис: Краще мати багато спеціалізованих інтерфейсів, ніж один універсальний.

Приклади в коді:
- **[IPlayerRepository.cs специфічні методи для гравців](https://github.com/vt241pav-netizen/refnba_lab1/blob/11165d737a1bd18b764b4c18eaa122eb620fbe83/NBA.EFCore/Repositories/IPlayerRepository.cs#L17-L20)**


### D - Dependency Inversion Principle (Принцип інверсії залежностей)
Опис: Залежність від абстракцій, а не конкретних реалізацій.

Приклади в коді:
- **[NbaService.cs - залежить від абстракції NbaDbContext](https://github.com/vt241pav-netizen/refnba_lab1/blob/11165d737a1bd18b764b4c18eaa122eb620fbe83/NBA.EFCore/Services/NbaService.cs#L11-L18).**

## 2. DRY (Don't Repeat Yourself)
Опис: Уникайте дублювання коду.

Приклади в коді:
- **[Cпільні поля для всіх сутностей](https://github.com/vt241pav-netizen/refnba_lab1/blob/11165d737a1bd18b764b4c18eaa122eb620fbe83/NBA.EFCore/EFModels/Team.cs#L54C1-L54C40).**

## 3. KISS (Keep It Simple, Stupid)
Опис: Код має бути простим і зрозумілим.

Приклади в коді:
- **[Кожен метод виконує одну чітку задачу](https://github.com/vt241pav-netizen/refnba_lab1/blob/11165d737a1bd18b764b4c18eaa122eb620fbe83/NBA.EFCore/Program.cs#L886-L966).**

## 4. YAGNI (You Ain't Gonna Need It)
Опис: Не додавайте функціональність, яка поки не потрібна.
Приклади в коді:
Тільки CRUD операції - додані тільки потрібні методи
Soft delete - реалізовано тільки необхідні фільтри

## 5. Fail Fast
Опис: Перевіряйте помилки якомога раніше.
Приклади в коді:
- **[TryParse перевірки](https://github.com/vt241pav-netizen/refnba_lab1/blob/11165d737a1bd18b764b4c18eaa122eb620fbe83/NBA.EFCore/Program.cs#L972-L978)**
Валідація в CreateAsync


## 6. Separation of Concerns (Розділення відповідальності)
Опис: Різні аспекти програми мають бути розділені.

Приклади в коді:
Репозиторії - робота з БД
Сервіси - бізнес-логіка
DTO - передача даних
Program.cs - UI логіка

## 7. Dependency Injection
Опис: Залежності мають передаватися ззовні.
Приклади в коді:
Program.cs - створення всіх залежностей
Конструктори репозиторіїв

## 8. Composition over Inheritance
Опис: Композиція краще за наслідування.
Приклади в коді:
NbaService використовує репозиторії - композиція замість наслідування
Program.cs використовує сервіси

