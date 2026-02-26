using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NBA.EFCore.Data;
using NBA.EFCore.DTOs;
using NBA.EFCore.EFModels;
using NBA.EFCore.Repositories;
using NBA.EFCore.Services;
using System.Text;
using System.Linq;
using NBA.EFCore.Exceptions;

class Program
{
    private static NbaDbContext _context = null!;
    private static IAuthService _authService = null!;
    private static ITeamRepository _teamRepository = null!;
    private static ICoachRepository _coachRepository = null!;
    private static IMatchRepository _matchRepository = null!;
    private static IPlayerRepository _playerRepository = null!;
    private static IStatisticRepository _statisticRepository = null!;
    private static User? _currentUser;
    private static StatisticInputService _statisticInputService = null!;
    private static StatisticTransactionService _statisticTransactionService = null!;
    

    static async Task Main(string[] args)
{
    Console.OutputEncoding = Encoding.UTF8;
    
    var connectionString = "Server=.;Database=database_nba;Trusted_Connection=True;TrustServerCertificate=True";
    var optionsBuilder = new DbContextOptionsBuilder<NbaDbContext>();
    optionsBuilder.UseSqlServer(connectionString);
    
    _context = new NbaDbContext(optionsBuilder.Options);
    
    _authService = new AuthService(_context);
    _teamRepository = new TeamRepository(_context);
    _coachRepository = new CoachRepository(_context);
    _matchRepository = new MatchRepository(_context);
    _playerRepository = new PlayerRepository(_context);
    _statisticRepository = new StatisticRepository(_context);
    _statisticInputService = new StatisticInputService(_context);
    _statisticTransactionService = new StatisticTransactionService(_context);
    
    Console.Clear();
    
    try
    {
        await _authService.InitializeTestUsersAsync();
        await RunMainMenu();
    }
    catch (Exception ex)
    {
        PrintError($"Критична помилка: {ex.Message}");
        Console.ReadKey();
    }
}

    static async Task RunMainMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("NBA DATABASE MANAGEMENT SYSTEM");
            
            if (_currentUser == null)
            {
                Console.WriteLine("1. Увійти в систему");
                Console.WriteLine("2. Вийти");
            }
            else
            {
                Console.WriteLine($"Користувач: {_currentUser.Username} (Роль: {_currentUser.UserRole})");
                
                Console.WriteLine("1. Управління командами");
                Console.WriteLine("2. Управління тренерами");
                Console.WriteLine("3. Управління гравцями");
                Console.WriteLine("4. Управління матчами");
                Console.WriteLine("5. Управління статистикою");
                Console.WriteLine("6. Звіти та складні запити");
                
                if (_currentUser.UserRole == "Admin")
                {
                    Console.WriteLine("7. Адмін-панель (управління видаленими)");
                }
                
                Console.WriteLine("8. Вийти з системи");
            }

            Console.Write("\nОберіть опцію: ");
            var choice = Console.ReadLine();

            if (_currentUser == null)
            {
                switch (choice)
                {
                    case "1":
                        await Login();
                        break;
                    case "2":
                        Environment.Exit(0);
                        break;
                }
            }
            else
            {
                switch (choice)
                {
                    case "1":
                        await ManageTeams();
                        break;
                    case "2":
                        await ManageCoaches();
                        break;
                    case "3":
                        await ManagePlayers();
                        break;
                    case "4":
                        await ManageMatches();
                        break;
                    case "5":
                        await ManageStatistics();
                        break;
                    case "6":
                        await ShowReports();
                        break;
                    case "7":
                        if (_currentUser.UserRole == "Admin")
                            await AdminDeletedItemsMenu();
                        else
                            Console.WriteLine("Невірний вибір!");
                        break;
                    case "8":
                        _currentUser = null;
                        Console.WriteLine("Ви вийшли з системи.");
                        Console.ReadKey();
                        break;
                    default:
                        Console.WriteLine("Невірний вибір!");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }

    static async Task Login()
    {
        Console.Clear();
        PrintHeader(" ВХІД ДО СИСТЕМИ");

        Console.Write("Ім'я користувача: ");
        var username = Console.ReadLine();
        
        Console.Write("Пароль: ");
        var password = ReadPassword();
        
        var loginDto = new LoginDto
        {
            Username = username ?? "",
            Password = password
        };
        
        _currentUser = await _authService.AuthenticateAsync(loginDto);
        
        if (_currentUser != null)
        {
            PrintSuccess($"Успішний вхід! Ласкаво просимо, {_currentUser.Username}!");
            Console.ReadKey();
        }
        else
        {
            PrintError("Невірне ім'я користувача або пароль!");
            Console.ReadKey();
        }
    }

    static async Task ManageTeams()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"УПРАВЛІННЯ КОМАНДАМИ ({_currentUser?.UserRole})");
            
            Console.WriteLine("1. Переглянути всі команди");
            Console.WriteLine("2. Пошук команди за ID");
            
            if (_currentUser?.UserRole == "Analyst")
            {
                Console.WriteLine("3. Назад");
                Console.WriteLine("\nПримітка: Аналіст може тільки переглядати дані");
            }
            else if (_currentUser?.UserRole == "Developer" || _currentUser?.UserRole == "Admin")
            {
                Console.WriteLine("3. Додати нову команду");
                Console.WriteLine("4. Редагувати команду");
                Console.WriteLine("5. Видалити команду (Soft Delete)");
                Console.WriteLine("6. Назад");
            }
            
            Console.Write("\nОберіть опцію: ");
            var choice = Console.ReadLine();
            
            if (_currentUser?.UserRole == "Analyst")
            {
                switch (choice)
                {
                    case "1":
                        await ShowAllTeams();
                        break;
                    case "2":
                        await SearchTeam();
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Невірний вибір!");
                        Console.ReadKey();
                        break;
                }
            }
            else 
            {
                switch (choice)
                {
                    case "1":
                        await ShowAllTeams();
                        break;
                    case "2":
                        await SearchTeam();
                        break;
                    case "3":
                        await AddTeam();
                        break;
                    case "4":
                        await EditTeam();
                        break;
                    case "5":
                        await DeleteTeam();
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Невірний вибір!");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }

    static async Task ShowAllTeams()
    {
        Console.Clear();
        PrintHeader(" ВСІ КОМАНДИ");
        
        var teams = await _teamRepository.GetAllAsync();
        
        foreach (var team in teams)
        {
            Console.WriteLine($"\nID: {team.TeamId}");
            Console.WriteLine($"Назва: {team.TeamName} ({team.Abbreviation})");
            Console.WriteLine($"Арена: {team.Arena?.ArenaName ?? "Немає"}");
            Console.WriteLine($"Місто: {team.Arena?.City ?? "Немає"}");
            Console.WriteLine($"Кількість гравців: {team.Players?.Count ?? 0}");
            Console.WriteLine($"Кількість тренерів: {team.Coaches?.Count ?? 0}");
            Console.WriteLine("---");
        }
        
        Console.WriteLine($"\nВсього команд: {teams.Count}");
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task SearchTeam()
    {
        Console.Clear();
        PrintHeader(" ПОШУК КОМАНДИ ЗА ID");
        
        Console.Write("Введіть ID команди: ");
        if (!int.TryParse(Console.ReadLine(), out int teamId))
        {
            PrintError("Невірний ID!");
            Console.ReadKey();
            return;
        }
        
        var team = await _teamRepository.GetByIdAsync(teamId);
        
        if (team == null)
        {
            PrintError("Команда не знайдена!");
        }
        else
        {
            Console.WriteLine($"\nID: {team.TeamId}");
            Console.WriteLine($"Назва: {team.TeamName} ({team.Abbreviation})");
            Console.WriteLine($"Арена: {team.Arena?.ArenaName}");
            Console.WriteLine($"Місто: {team.Arena?.City}");
            Console.WriteLine($"Рік заснування: {team.YearFounded}");
            Console.WriteLine($"Генеральний менеджер: {team.GeneralManager}");
            Console.WriteLine($"Конференція ID: {team.ConferenceId}");
        }
        
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task AddTeam()
    {
        if (_currentUser?.UserRole == "Analyst")
        {
            Console.WriteLine("Доступ заборонено! Аналіст може тільки переглядати дані.");
            Console.ReadKey();
            return;
        }
        
        Console.Clear();
        PrintHeader(" ДОДАВАННЯ КОМАНДИ");
        
        try
        {
            Console.Write("ID команди: ");
            if (!int.TryParse(Console.ReadLine(), out int teamId))
            {
                PrintError("Невірний ID!");
                Console.ReadKey();
                return;
            }
            
            Console.Write("Назва команди: ");
            var name = Console.ReadLine();
            
            Console.Write("Абревіатура: ");
            var abbreviation = Console.ReadLine();
            
            Console.Write("ID арени: ");
            if (!int.TryParse(Console.ReadLine(), out int arenaId))
            {
                PrintError("Невірний ID арени!");
                Console.ReadKey();
                return;
            }
            
            Console.Write("ID дивізіону: ");
            if (!int.TryParse(Console.ReadLine(), out int divisionId))
            {
                PrintError("Невірний ID дивізіону!");
                Console.ReadKey();
                return;
            }
            
            Console.Write("ID конференції: ");
            if (!int.TryParse(Console.ReadLine(), out int conferenceId))
            {
                PrintError("Невірний ID конференції!");
                Console.ReadKey();
                return;
            }
            
            var team = new Team
            {
                TeamId = teamId,
                TeamName = name ?? "",
                Abbreviation = abbreviation ?? "",
                ArenaId = arenaId,
                DivisionId = divisionId,
                ConferenceId = conferenceId,
                YearFounded = DateOnly.FromDateTime(DateTime.Now),
                IsDeleted = false
            };
            
            await _teamRepository.CreateAsync(team);
            PrintSuccess("Команда успішно додана!");
        }
        catch (Exception ex)
        {
            PrintError($"Помилка: {ex.Message}");
        }
        
        Console.ReadKey();
    }

    static async Task EditTeam()
    {
        if (_currentUser?.UserRole == "Analyst")
        {
            Console.WriteLine("Доступ заборонено! Аналіст може тільки переглядати дані.");
            Console.ReadKey();
            return;
        }
        
        Console.Clear();
        PrintHeader(" РЕДАГУВАННЯ КОМАНДИ");
        
        Console.Write("Введіть ID команди для редагування: ");
        if (!int.TryParse(Console.ReadLine(), out int teamId))
        {
            PrintError("Невірний ID!");
            Console.ReadKey();
            return;
        }
        
        var team = await _teamRepository.GetByIdAsync(teamId);
        
        if (team == null)
        {
            PrintError("Команда не знайдена!");
            Console.ReadKey();
            return;
        }
        
        Console.WriteLine($"\nПоточна назва: {team.TeamName}");
        Console.Write("Нова назва (Enter для залишення поточної): ");
        var newName = Console.ReadLine();
        if (!string.IsNullOrEmpty(newName))
            team.TeamName = newName;
        
        Console.WriteLine($"\nПоточна абревіатура: {team.Abbreviation}");
        Console.Write("Нова абревіатура (Enter для залишення поточної): ");
        var newAbbr = Console.ReadLine();
        if (!string.IsNullOrEmpty(newAbbr))
            team.Abbreviation = newAbbr;
        
        Console.WriteLine($"\nПоточний ID арени: {team.ArenaId}");
        Console.Write("Новий ID арени (Enter для залишення поточного): ");
        var arenaInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(arenaInput) && int.TryParse(arenaInput, out int newArenaId))
            team.ArenaId = newArenaId;
        
        try
        {
            await _teamRepository.UpdateAsync(team);
            PrintSuccess("Команда успішно оновлена!");
        }
        catch (Exception ex)
        {
            PrintError($"Помилка: {ex.Message}");
        }
        
        Console.ReadKey();
    }

    static async Task DeleteTeam()
    {
        if (_currentUser?.UserRole != "Admin")
        {
            Console.WriteLine("Доступ заборонено! Видалення команд тільки для адміністраторів.");
            Console.ReadKey();
            return;
        }
        
        Console.Clear();
        PrintHeader(" ВИДАЛЕННЯ КОМАНДИ (SOFT DELETE)");
        
        Console.Write("Введіть ID команди для видалення: ");
        if (!int.TryParse(Console.ReadLine(), out int teamId))
        {
            PrintError("Невірний ID!");
            Console.ReadKey();
            return;
        }
        
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
        {
            PrintError("Команда не знайдена!");
            Console.ReadKey();
            return;
        }
        
        Console.WriteLine($"\nІнформація про команду:");
        Console.WriteLine($"  Назва: {team.TeamName}");
        Console.WriteLine($"  Гравців: {team.Players?.Count ?? 0}");
        Console.WriteLine($"  Тренерів: {team.Coaches?.Count ?? 0}");
        
        
        Console.Write($"\nВидалити команду {team.TeamName} та всіх її гравців/тренерів? (yes/no): ");
        var confirm = Console.ReadLine()?.ToLower();
        
        if (confirm == "yes" || confirm == "y")
        {
            try
            {
                await _teamRepository.DeleteAsync(teamId);
                PrintSuccess($"Команду {team.TeamName} та всіх її гравців/тренерів успішно видалено!");
            }
            catch (Exception ex)
            {
                PrintError($"Помилка: {ex.Message}");
            }
        }
        else
        {
            PrintInfo("Видалення скасовано.");
        }
        
        Console.ReadKey();
    }

    static async Task ManageCoaches()
    {
        while (true)
        {
            Console.Clear();
            PrintHeader(" УПРАВЛІННЯ ТРЕНЕРАМИ");
            
            Console.WriteLine("1. Переглянути всіх тренерів");
            Console.WriteLine("2. Пошук тренера за ID");
            
            if (_currentUser?.UserRole == "Analyst")
            {
                Console.WriteLine("3. Назад");
            }
            else
            {
                Console.WriteLine("3. Додати нового тренера");
                Console.WriteLine("4. Редагувати тренера");
                Console.WriteLine("5. Видалити тренера (Soft Delete)");
                Console.WriteLine("6. Назад");
            }
            
            Console.Write("\nОберіть опцію: ");
            var choice = Console.ReadLine();
            
            if (_currentUser?.UserRole == "Analyst")
            {
                switch (choice)
                {
                    case "1":
                        await ShowAllCoaches();
                        break;
                    case "2":
                        await SearchCoach();
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Невірний вибір!");
                        Console.ReadKey();
                        break;
                }
            }
            else
            {
                switch (choice)
                {
                    case "1":
                        await ShowAllCoaches();
                        break;
                    case "2":
                        await SearchCoach();
                        break;
                    case "3":
                        await AddCoach();
                        break;
                    case "4":
                        await EditCoach();
                        break;
                    case "5":
                        await DeleteCoach();
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Невірний вибір!");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }

    static async Task ShowAllCoaches()
    {
        Console.Clear();
        PrintHeader(" ВСІ ТРЕНЕРИ");
        
        var coaches = await _coachRepository.GetAllAsync();
        
        foreach (var coach in coaches)
        {
            Console.WriteLine($"\nID: {coach.CoachId}");
            Console.WriteLine($"Ім'я: {coach.FirstName} {coach.LastName}");
            Console.WriteLine($"Команда: {coach.Team?.TeamName ?? "Немає"}");
            Console.WriteLine($"Роль: {coach.Role}");
            Console.WriteLine($"Дата початку: {coach.StartDate}");
            Console.WriteLine($"Дата завершення: {coach.EndDate}");
            Console.WriteLine("---");
        }
        
        Console.WriteLine($"\nВсього тренерів: {coaches.Count}");
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task SearchCoach()
    {
        Console.Clear();
        PrintHeader(" ПОШУК ТРЕНЕРА ЗА ID");
        
        Console.Write("Введіть ID тренера: ");
        if (!int.TryParse(Console.ReadLine(), out int coachId))
        {
            PrintError("Невірний ID!");
            Console.ReadKey();
            return;
        }
        
        var coach = await _coachRepository.GetByIdAsync(coachId);
        
        if (coach == null)
        {
            PrintError("Тренер не знайдений!");
        }
        else
        {
            Console.WriteLine($"\nID: {coach.CoachId}");
            Console.WriteLine($"Ім'я: {coach.FirstName} {coach.LastName}");
            Console.WriteLine($"Команда: {coach.Team?.TeamName}");
            Console.WriteLine($"Роль: {coach.Role}");
            Console.WriteLine($"Дата початку: {coach.StartDate}");
            Console.WriteLine($"Дата завершення: {coach.EndDate}");
        }
        
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task AddCoach()
    {
        if (_currentUser?.UserRole == "Analyst")
        {
            Console.WriteLine("Доступ заборонено! Аналіст може тільки переглядати дані.");
            Console.ReadKey();
            return;
        }
        
        Console.Clear();
        PrintHeader(" ДОДАВАННЯ ТРЕНЕРА");
        
        try
        {
            Console.Write("ID тренера: ");
            if (!int.TryParse(Console.ReadLine(), out int coachId))
            {
                PrintError("Невірний ID!");
                Console.ReadKey();
                return;
            }
            
            Console.Write("ID команди: ");
            if (!int.TryParse(Console.ReadLine(), out int teamId))
            {
                PrintError("Невірний ID команди!");
                Console.ReadKey();
                return;
            }
            
            Console.Write("Ім'я: ");
            var firstName = Console.ReadLine();
            
            Console.Write("Прізвище: ");
            var lastName = Console.ReadLine();
            
            Console.Write("Роль: ");
            var role = Console.ReadLine();
            
            Console.Write("Рік початку: ");
            if (!int.TryParse(Console.ReadLine(), out int startYear))
            {
                startYear = DateTime.Now.Year;
            }
            
            var coach = new Coach
            {
                CoachId = coachId,
                TeamId = teamId,
                FirstName = firstName ?? "",
                LastName = lastName ?? "",
                Role = role ?? "",
                StartDate = new DateOnly(startYear, 1, 1),
                EndDate = new DateOnly(startYear + 2, 12, 31),
                IsDeleted = false
            };
            
            await _coachRepository.CreateAsync(coach);
            PrintSuccess("Тренера успішно додано!");
        }
        catch (Exception ex)
        {
            PrintError($"Помилка: {ex.Message}");
        }
        
        Console.ReadKey();
    }

    static async Task EditCoach()
    {
        if (_currentUser?.UserRole == "Analyst")
        {
            Console.WriteLine("Доступ заборонено! Аналіст може тільки переглядати дані.");
            Console.ReadKey();
            return;
        }
        
        Console.Clear();
        PrintHeader(" РЕДАГУВАННЯ ТРЕНЕРА");
        
        Console.Write("Введіть ID тренера для редагування: ");
        if (!int.TryParse(Console.ReadLine(), out int coachId))
        {
            PrintError("Невірний ID!");
            Console.ReadKey();
            return;
        }
        
        var coach = await _coachRepository.GetByIdAsync(coachId);
        
        if (coach == null)
        {
            PrintError("Тренер не знайдений!");
            Console.ReadKey();
            return;
        }
        
        Console.WriteLine($"\nПоточне ім'я: {coach.FirstName}");
        Console.Write("Нове ім'я (Enter для залишення поточного): ");
        var newFirstName = Console.ReadLine();
        if (!string.IsNullOrEmpty(newFirstName))
            coach.FirstName = newFirstName;
        
        Console.WriteLine($"\nПоточне прізвище: {coach.LastName}");
        Console.Write("Нове прізвище (Enter для залишення поточного): ");
        var newLastName = Console.ReadLine();
        if (!string.IsNullOrEmpty(newLastName))
            coach.LastName = newLastName;
        
        Console.WriteLine($"\nПоточна роль: {coach.Role}");
        Console.Write("Нова роль (Enter для залишення поточної): ");
        var newRole = Console.ReadLine();
        if (!string.IsNullOrEmpty(newRole))
            coach.Role = newRole;
        
        try
        {
            await _coachRepository.UpdateAsync(coach);
            PrintSuccess("Тренера успішно оновлено!");
        }
        catch (Exception ex)
        {
            PrintError($"Помилка: {ex.Message}");
        }
        
        Console.ReadKey();
    }

    static async Task DeleteCoach()
    {
        if (_currentUser?.UserRole != "Admin")
        {
            Console.WriteLine("Доступ заборонено! Видалення тільки для адміністраторів.");
            Console.ReadKey();
            return;
        }
        
        Console.Clear();
        PrintHeader(" ВИДАЛЕННЯ ТРЕНЕРА (SOFT DELETE)");
        
        Console.Write("Введіть ID тренера для видалення: ");
        if (!int.TryParse(Console.ReadLine(), out int coachId))
        {
            PrintError("Невірний ID!");
            Console.ReadKey();
            return;
        }
        
        var coach = await _coachRepository.GetByIdAsync(coachId);
        if (coach == null)
        {
            PrintError("Тренер не знайдений!");
            Console.ReadKey();
            return;
        }
        
        Console.WriteLine($"\nІнформація про тренера:");
        Console.WriteLine($"  Ім'я: {coach.FirstName} {coach.LastName}");
        Console.WriteLine($"  Команда: {coach.Team?.TeamName ?? "Немає"}");
        Console.WriteLine($"  Роль: {coach.Role}");
        
        Console.Write($"\nВидалити тренера {coach.FirstName} {coach.LastName}? (y/n): ");
        var confirm = Console.ReadLine();
        
        if (confirm?.ToLower() == "y")
        {
            try
            {
                await _coachRepository.DeleteAsync(coachId);
                PrintSuccess($"Тренера {coach.FirstName} {coach.LastName} успішно видалено (Soft Delete)!");
            }
            catch (Exception ex)
            {
                PrintError($"Помилка: {ex.Message}");
            }
        }
        else
        {
            PrintInfo("Видалення скасовано.");
        }
        
        Console.ReadKey();
    }

    static async Task ManagePlayers()
    {
        while (true)
        {
            Console.Clear();
            PrintHeader(" УПРАВЛІННЯ ГРАВЦЯМИ");
            
            Console.WriteLine("1. Переглянути всіх гравців");
            Console.WriteLine("2. Пошук гравця за ID");
            
            if (_currentUser?.UserRole == "Analyst")
            {
                Console.WriteLine("3. Назад");
            }
            else
            {
                Console.WriteLine("3. Додати нового гравця");
                Console.WriteLine("4. Редагувати гравця");
                Console.WriteLine("5. Видалити гравця (Soft Delete)");
                Console.WriteLine("6. Переглянути статистику гравця");
                Console.WriteLine("7. Назад");
            }
            
            Console.Write("\nОберіть опцію: ");
            var choice = Console.ReadLine();
            
            if (_currentUser?.UserRole == "Analyst")
            {
                switch (choice)
                {
                    case "1":
                        await ShowAllPlayers();
                        break;
                    case "2":
                        await SearchPlayer();
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Невірний вибір!");
                        Console.ReadKey();
                        break;
                }
            }
            else
            {
                switch (choice)
                {
                    case "1":
                        await ShowAllPlayers();
                        break;
                    case "2":
                        await SearchPlayer();
                        break;
                    case "3":
                        await AddPlayer();
                        break;
                    case "4":
                        await EditPlayer();
                        break;
                    case "5":
                        await DeletePlayer();
                        break;
                    case "6":
                        await ShowPlayerStats();
                        break;
                    case "7":
                        return;
                    default:
                        Console.WriteLine("Невірний вибір!");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }

    static async Task ShowAllPlayers()
    {
        Console.Clear();
        Console.WriteLine(" ВСІ ГРАВЦІ ");
        
        var players = await _playerRepository.GetActivePlayersAsync();
        
        if (players.Count == 0)
        {
            Console.WriteLine("Активних гравців не знайдено.");
            
            if (_currentUser?.UserRole == "Admin")
            {
                var deletedPlayers = await _playerRepository.GetDeletedPlayersAsync();
                
                if (deletedPlayers.Any())
                {
                    Console.WriteLine($"\n  Знайдено {deletedPlayers.Count} видалених гравців:");
                    foreach (var player in deletedPlayers.Take(10))
                    {
                        Console.WriteLine($"  {player.FirstName} {player.LastName} (ID: {player.PlayerId})");
                    }
                    
                    if (deletedPlayers.Count > 10)
                        Console.WriteLine($"  ... і ще {deletedPlayers.Count - 10} гравців");
                }
            }
            
            Console.ReadKey();
            return;
        }
        
        Console.WriteLine($"\n Активних гравців: {players.Count}");
        Console.WriteLine($"{"ID",-6} {"Гравець",-25} {"Команда",-20} {"Позиція",-12} {"Вік",-5} {"Країна",-15}");
        Console.WriteLine(new string('─', 85));
        
        foreach (var player in players)
        {
            string playerName = $"{player.FirstName} {player.LastName}";
            if (playerName.Length > 22)
                playerName = playerName.Substring(0, 19) + "...";
                
            string teamName = player.Team?.TeamName ?? "Без команди";
            if (teamName.Length > 18)
                teamName = teamName.Substring(0, 15) + "...";
                
            int age = DateTime.Now.Year - player.BirthDate.Year;
            
            Console.WriteLine($"{player.PlayerId,-6} {playerName,-25} {teamName,-20} " +
                             $"{player.Position,-12} {age,-5} {player.Country,-15}");
        }
        
        if (_currentUser?.UserRole == "Admin")
        {
            var deletedPlayers = await _playerRepository.GetDeletedPlayersAsync();
            
            if (deletedPlayers.Count > 0)
            {
                Console.WriteLine($"\nІнформація для адміністратора:");
                Console.WriteLine($"  Видалених гравців (Soft Delete): {deletedPlayers.Count}");
                Console.Write("  Показати видалених гравців? (y/n): ");
                
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    Console.WriteLine($"\n  ОСТАННІ ВИДАЛЕНІ ГРАВЦІ:");
                    foreach (var player in deletedPlayers.Take(20))
                    {
                        Console.WriteLine($"  {player.PlayerId}: {player.FirstName} {player.LastName} " +
                                        $"- {player.Team?.TeamName ?? "Без команди"}");
                    }
                    
                    if (deletedPlayers.Count > 20)
                        Console.WriteLine($"  ... і ще {deletedPlayers.Count - 20} гравців");
                }
            }
        }
        
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task SearchPlayer()
    {
        Console.Clear();
        PrintHeader("ПОШУК ГРАВЦЯ ЗА ID");
        
        Console.Write("Введіть ID гравця: ");
        if (!int.TryParse(Console.ReadLine(), out int playerId))
        {
            PrintError("Невірний ID!");
            Console.ReadKey();
            return;
        }
        
        var player = await _playerRepository.GetByIdAsync(playerId);
        
        if (player == null)
        {
            PrintError("Гравець не знайдений!");
        }
        else
        {
            Console.WriteLine($"\nID: {player.PlayerId}");
            Console.WriteLine($"Ім'я: {player.FirstName} {player.LastName}");
            Console.WriteLine($"Команда: {player.Team?.TeamName}");
            Console.WriteLine($"Позиція: {player.Position}");
            Console.WriteLine($"Номер: {player.JerseyNumber}");
            Console.WriteLine($"Дата народження: {player.BirthDate}");
            Console.WriteLine($"Країна: {player.Country}");
            Console.WriteLine($"Зріст: {player.HeightCm} см");
            Console.WriteLine($"Вага: {player.WeightKg} кг");
            Console.WriteLine($"Рік драфту: {player.DraftYear}");
        }
        
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task AddPlayer()
    {
        if (_currentUser?.UserRole == "Analyst")
        {
            Console.WriteLine("Доступ заборонено! Аналіст може тільки переглядати дані.");
            Console.ReadKey();
            return;
        }
        
        Console.Clear();
        PrintHeader(" ДОДАВАННЯ ГРАВЦЯ");
        
        try
        {
            Console.Write("ID гравця: ");
            if (!int.TryParse(Console.ReadLine(), out int playerId))
            {
                PrintError("Невірний ID!");
                Console.ReadKey();
                return;
            }
            
            Console.Write("Ім'я: ");
            var firstName = Console.ReadLine();
            
            Console.Write("Прізвище: ");
            var lastName = Console.ReadLine();
            
            Console.Write("ID команди: ");
            if (!int.TryParse(Console.ReadLine(), out int teamId))
            {
                PrintError("Невірний ID команди!");
                Console.ReadKey();
                return;
            }
            
            Console.Write("Позиція: ");
            var position = Console.ReadLine();
            
            Console.Write("Номер на майці: ");
            if (!int.TryParse(Console.ReadLine(), out int jerseyNumber))
            {
                PrintError("Невірний номер!");
                Console.ReadKey();
                return;
            }
            
            Console.Write("Рік народження (yyyy): ");
            if (!int.TryParse(Console.ReadLine(), out int birthYear))
            {
                PrintError("Невірний рік!");
                Console.ReadKey();
                return;
            }
            
            Console.Write("Країна: ");
            var country = Console.ReadLine();
            
            Console.Write("Зріст (см): ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal height))
            {
                PrintError("Невірний зріст!");
                Console.ReadKey();
                return;
            }
            
            Console.Write("Вага (кг): ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal weight))
            {
                PrintError("Невірна вага!");
                Console.ReadKey();
                return;
            }
            
            var player = new Player
            {
                PlayerId = playerId,
                FirstName = firstName ?? "",
                LastName = lastName ?? "",
                TeamId = teamId,
                Position = position ?? "",
                JerseyNumber = jerseyNumber,
                BirthDate = new DateOnly(birthYear, 1, 1),
                Country = country ?? "",
                HeightCm = height,
                WeightKg = weight,
                DraftYear = 2023,
                DraftRound = 1,
                DraftPick = 1,
                IsDeleted = false
            };
            
            await _playerRepository.CreateAsync(player);
            PrintSuccess("Гравця успішно додано!");
        }
        catch (Exception ex)
        {
            PrintError($"Помилка: {ex.Message}");
        }
        
        Console.ReadKey();
    }

    static async Task EditPlayer()
    {
        if (_currentUser?.UserRole == "Analyst")
        {
            Console.WriteLine("Доступ заборонено! Аналіст може тільки переглядати дані.");
            Console.ReadKey();
            return;
        }
        
        Console.Clear();
        PrintHeader(" РЕДАГУВАННЯ ГРАВЦЯ");
        
        Console.Write("Введіть ID гравця для редагування: ");
        if (!int.TryParse(Console.ReadLine(), out int playerId))
        {
            PrintError("Невірний ID!");
            Console.ReadKey();
            return;
        }
        
        var player = await _playerRepository.GetByIdAsync(playerId);
        
        if (player == null)
        {
            PrintError("Гравець не знайдений!");
            Console.ReadKey();
            return;
        }
        
        Console.WriteLine($"\nПоточне ім'я: {player.FirstName}");
        Console.Write("Нове ім'я (Enter для залишення поточного): ");
        var newFirstName = Console.ReadLine();
        if (!string.IsNullOrEmpty(newFirstName))
            player.FirstName = newFirstName;
        
        Console.WriteLine($"\nПоточне прізвище: {player.LastName}");
        Console.Write("Нове прізвище (Enter для залишення поточного): ");
        var newLastName = Console.ReadLine();
        if (!string.IsNullOrEmpty(newLastName))
            player.LastName = newLastName;
        
        Console.WriteLine($"\nПоточна позиція: {player.Position}");
        Console.Write("Нова позиція (Enter для залишення поточної): ");
        var newPosition = Console.ReadLine();
        if (!string.IsNullOrEmpty(newPosition))
            player.Position = newPosition;
        
        Console.WriteLine($"\nПоточний номер: {player.JerseyNumber}");
        Console.Write("Новий номер (Enter для залишення поточного): ");
        var jerseyInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(jerseyInput) && int.TryParse(jerseyInput, out int newJersey))
            player.JerseyNumber = newJersey;
        
        try
        {
            await _playerRepository.UpdateAsync(player);
            PrintSuccess("Гравця успішно оновлено!");
        }
        catch (Exception ex)
        {
            PrintError($"Помилка: {ex.Message}");
        }
        
        Console.ReadKey();
    }

    static async Task DeletePlayer()
    {
        if (_currentUser?.UserRole == "Analyst")
        {
            Console.WriteLine("Доступ заборонено! Аналіст може тільки переглядати дані.");
            Console.ReadKey();
            return;
        }
        
        Console.Clear();
        PrintHeader(" ВИДАЛЕННЯ ГРАВЦЯ (SOFT DELETE)");
        
        Console.Write("Введіть ID гравця для видалення: ");
        if (!int.TryParse(Console.ReadLine(), out int playerId))
        {
            PrintError("Невірний ID!");
            Console.ReadKey();
            return;
        }
        
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            PrintError("Гравець не знайдений!");
            Console.ReadKey();
            return;
        }
        
        Console.WriteLine($"\nІнформація про гравця:");
        Console.WriteLine($"  Ім'я: {player.FirstName} {player.LastName}");
        Console.WriteLine($"  Команда: {player.Team?.TeamName ?? "Немає"}");
        Console.WriteLine($"  Позиція: {player.Position}");
        
        Console.Write($"\nВидалити гравця {player.FirstName} {player.LastName}? (y/n): ");
        var confirm = Console.ReadLine();
        
        if (confirm?.ToLower() == "y")
        {
            try
            {
                await _playerRepository.DeleteAsync(playerId);
                PrintSuccess($"Гравця {player.FirstName} {player.LastName} успішно видалено (Soft Delete)!");
            }
            catch (Exception ex)
            {
                PrintError($"Помилка: {ex.Message}");
            }
        }
        else
        {
            PrintInfo("Видалення скасовано.");
        }
        
        Console.ReadKey();
    }

    static async Task ShowPlayerStats()
    {
        Console.Clear();
        PrintHeader(" СТАТИСТИКА ГРАВЦЯ");
        
        Console.Write("Введіть ID гравця: ");
        if (!int.TryParse(Console.ReadLine(), out int playerId))
        {
            PrintError("Невірний ID!");
            Console.ReadKey();
            return;
        }
        
        var player = await _playerRepository.GetByIdAsync(playerId);
        
        if (player == null)
        {
            PrintError("Гравець не знайдений!");
            Console.ReadKey();
            return;
        }
        
        Console.WriteLine($"\nГравець: {player.FirstName} {player.LastName}");
        Console.WriteLine($"Команда: {player.Team?.TeamName}");
        
        var statistics = await _statisticRepository.GetPlayerStatisticsAsync(playerId);
        
        if (statistics.Count == 0)
        {
            Console.WriteLine("\nСтатистика відсутня.");
        }
        else
        {
            Console.WriteLine("\nСтатистика:");
            foreach (var stat in statistics)
            {
                Console.WriteLine($"\nМатч ID: {stat.MatchId}");
                Console.WriteLine($"Очки: {stat.Points}");
                Console.WriteLine($"Підбирання: {stat.Rebounds}");
                Console.WriteLine($"Результативні передачі: {stat.Assists}");
                Console.WriteLine($"Перехоплення: {stat.Steals}");
                Console.WriteLine($"Блокшоти: {stat.Blocks}");
                Console.WriteLine($"Втрати: {stat.Turnovers}");
                Console.WriteLine("---");
            }
        }
        
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task ManageMatches()
    {
        while (true)
        {
            Console.Clear();
            PrintHeader(" УПРАВЛІННЯ МАТЧАМИ");
            
            Console.WriteLine("1. Переглянути всі матчі");
            Console.WriteLine("2. Пошук матчу за ID");
            
            if (_currentUser?.UserRole == "Analyst")
            {
                Console.WriteLine("3. Назад");
            }
            else
            {
                Console.WriteLine("3. Додати новий матч");
                Console.WriteLine("4. Редагувати матч");
                Console.WriteLine("5. Видалити матч (Soft Delete)");
                Console.WriteLine("6. Матчі за датою");
                Console.WriteLine("7. Назад");
            }
            
            Console.Write("\nОберіть опцію: ");
            var choice = Console.ReadLine();
            
            if (_currentUser?.UserRole == "Analyst")
            {
                switch (choice)
                {
                    case "1":
                        await ShowAllMatches();
                        break;
                    case "2":
                        await SearchMatch();
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Невірний вибір!");
                        Console.ReadKey();
                        break;
                }
            }
            else
            {
                switch (choice)
                {
                    case "1":
                        await ShowAllMatches();
                        break;
                    case "2":
                        await SearchMatch();
                        break;
                    case "3":
                        await AddMatch();
                        break;
                    case "4":
                        await EditMatch();
                        break;
                    case "5":
                        await DeleteMatch();
                        break;
                    case "6":
                        await ShowMatchesByDate();
                        break;
                    case "7":
                        return;
                    default:
                        Console.WriteLine("Невірний вибір!");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }

    static async Task ShowAllMatches()
    {
        Console.Clear();
        PrintHeader(" ВСІ МАТЧІ");
        
        var matches = await _matchRepository.GetAllAsync();
        
        foreach (var match in matches)
        {
            Console.WriteLine($"\nID: {match.MatchId}");
            Console.WriteLine($"Сезон: {match.Season}");
            Console.WriteLine($"Тип: {match.MatchType}");
            Console.WriteLine($"Дата: {match.GameDate:dd.MM.yyyy HH:mm}");
            Console.WriteLine($"Домашня команда ID: {match.HomeTeamId}");
            Console.WriteLine($"Гостьова команда ID: {match.AwayTeamId}");
            Console.WriteLine($"Рахунок: {match.HomeTeamScore}:{match.AwayTeamScore}");
            Console.WriteLine("---");
        }
        
        Console.WriteLine($"\nВсього матчів: {matches.Count}");
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task SearchMatch()
    {
        Console.Clear();
        PrintHeader(" ПОШУК МАТЧУ ЗА ID");
        
        Console.Write("Введіть ID матчу: ");
        if (!int.TryParse(Console.ReadLine(), out int matchId))
        {
            PrintError("Невірний ID!");
            Console.ReadKey();
            return;
        }
        
        var match = await _matchRepository.GetByIdAsync(matchId);
        
        if (match == null)
        {
            PrintError("Матч не знайдений!");
        }
        else
        {
            Console.WriteLine($"\nID: {match.MatchId}");
            Console.WriteLine($"Сезон: {match.Season}");
            Console.WriteLine($"Тип: {match.MatchType}");
            Console.WriteLine($"Дата: {match.GameDate:dd.MM.yyyy HH:mm}");
            Console.WriteLine($"Домашня команда ID: {match.HomeTeamId}");
            Console.WriteLine($"Гостьова команда ID: {match.AwayTeamId}");
            Console.WriteLine($"Рахунок: {match.HomeTeamScore}:{match.AwayTeamScore}");
        }
        
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task AddMatch()
    {
        if (_currentUser?.UserRole == "Analyst")
        {
            Console.WriteLine("Доступ заборонено! Аналіст може тільки переглядати дані.");
            Console.ReadKey();
            return;
        }
        
        Console.Clear();
        PrintHeader(" ДОДАВАННЯ МАТЧУ");
        
        try
        {
            Console.Write("ID матчу: ");
            if (!int.TryParse(Console.ReadLine(), out int matchId))
            {
                PrintError("Невірний ID!");
                Console.ReadKey();
                return;
            }
            
            Console.Write("Сезон (напр. 2023-2024): ");
            var season = Console.ReadLine();
            
            Console.Write("Тип матчу (Regular/Playoff): ");
            var matchType = Console.ReadLine();
            
            Console.Write("Дата (рррр-мм-дд): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime gameDate))
            {
                PrintError("Невірна дата!");
                Console.ReadKey();
                return;
            }
            
            Console.Write("ID домашньої команди: ");
            if (!int.TryParse(Console.ReadLine(), out int homeTeamId))
            {
                PrintError("Невірний ID!");
                Console.ReadKey();
                return;
            }
            
            Console.Write("ID гостьової команди: ");
            if (!int.TryParse(Console.ReadLine(), out int awayTeamId))
            {
                PrintError("Невірний ID!");
                Console.ReadKey();
                return;
            }
            
            Console.Write("Очки домашньої команди: ");
            if (!int.TryParse(Console.ReadLine(), out int homeScore))
            {
                PrintError("Невірний рахунок!");
                Console.ReadKey();
                return;
            }
            
            Console.Write("Очки гостьової команди: ");
            if (!int.TryParse(Console.ReadLine(), out int awayScore))
            {
                PrintError("Невірний рахунок!");
                Console.ReadKey();
                return;
            }
            
            var match = new Match
            {
                MatchId = matchId,
                Season = season ?? "",
                MatchType = matchType ?? "",
                GameDate = gameDate,
                HomeTeamId = homeTeamId,
                AwayTeamId = awayTeamId,
                HomeTeamScore = homeScore,
                AwayTeamScore = awayScore,
                IsDeleted = false
            };
            
            await _matchRepository.CreateAsync(match);
            PrintSuccess("Матч успішно додано!");
        }
        catch (Exception ex)
        {
            PrintError($"Помилка: {ex.Message}");
        }
        
        Console.ReadKey();
    }

    static async Task EditMatch()
    {
        if (_currentUser?.UserRole == "Analyst")
        {
            Console.WriteLine("Доступ заборонено! Аналіст може тільки переглядати дані.");
            Console.ReadKey();
            return;
        }
        
        Console.Clear();
        PrintHeader(" РЕДАГУВАННЯ МАТЧУ");
        
        Console.Write("Введіть ID матчу для редагування: ");
        if (!int.TryParse(Console.ReadLine(), out int matchId))
        {
            PrintError("Невірний ID!");
            Console.ReadKey();
            return;
        }
        
        var match = await _matchRepository.GetByIdAsync(matchId);
        
        if (match == null)
        {
            PrintError("Матч не знайдений!");
            Console.ReadKey();
            return;
        }
        
        Console.WriteLine($"\nПоточний рахунок: {match.HomeTeamScore}:{match.AwayTeamScore}");
        Console.Write("Нові очки домашньої команди (Enter для залишення поточних): ");
        var homeScoreInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(homeScoreInput) && int.TryParse(homeScoreInput, out int newHomeScore))
            match.HomeTeamScore = newHomeScore;
        
        Console.Write("Нові очки гостьової команди (Enter для залишення поточних): ");
        var awayScoreInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(awayScoreInput) && int.TryParse(awayScoreInput, out int newAwayScore))
            match.AwayTeamScore = newAwayScore;
        
        try
        {
            await _matchRepository.UpdateAsync(match);
            PrintSuccess("Матч успішно оновлено!");
        }
        catch (Exception ex)
        {
            PrintError($"Помилка: {ex.Message}");
        }
        
        Console.ReadKey();
    }

    static async Task DeleteMatch()
    {
        if (_currentUser?.UserRole != "Admin")
        {
            Console.WriteLine("Доступ заборонено! Видалення тільки для адміністраторів.");
            Console.ReadKey();
            return;
        }
        
        Console.Clear();
        PrintHeader(" ВИДАЛЕННЯ МАТЧУ (SOFT DELETE)");
        
        Console.Write("Введіть ID матчу для видалення: ");
        if (!int.TryParse(Console.ReadLine(), out int matchId))
        {
            PrintError("Невірний ID!");
            Console.ReadKey();
            return;
        }
        
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
        {
            PrintError("Матч не знайдений!");
            Console.ReadKey();
            return;
        }
        
        Console.WriteLine($"\nІнформація про матч:");
        Console.WriteLine($"  Дата: {match.GameDate:dd.MM.yyyy}");
        Console.WriteLine($"  Сезон: {match.Season}");
        Console.WriteLine($"  Тип: {match.MatchType}");
        Console.WriteLine($"  Рахунок: {match.HomeTeamScore}:{match.AwayTeamScore}");
        
        Console.Write($"\nВидалити матч від {match.GameDate:dd.MM.yyyy}? (y/n): ");
        var confirm = Console.ReadLine();
        
        if (confirm?.ToLower() == "y")
        {
            try
            {
                await _matchRepository.DeleteAsync(matchId);
                PrintSuccess($"Матч успішно видалено (Soft Delete)!");
            }
            catch (Exception ex)
            {
                PrintError($"Помилка: {ex.Message}");
            }
        }
        else
        {
            PrintInfo("Видалення скасовано.");
        }
        
        Console.ReadKey();
    }

    static async Task ShowMatchesByDate()
    {
        Console.Clear();
        Console.WriteLine("МАТЧІ ЗА ДАТОЮ");
        
        Console.WriteLine("1. Матчі за конкретну дату");
        Console.WriteLine("2. Матчі за період");
        Console.WriteLine("3. Назад");
        
        Console.Write("\nОберіть опцію: ");
        var choice = Console.ReadLine();
        
        switch (choice)
        {
            case "1":
                await ShowMatchesForSpecificDate();
                break;
            case "2":
                await ShowMatchesForDateRange();
                break;
            case "3":
                return;
        }
    }

    static async Task ShowMatchesForSpecificDate()
    {
        Console.Clear();
        Console.WriteLine("МАТЧІ ЗА КОНКРЕТНУ ДАТУ");
        
        Console.Write("Введіть дату (рррр-мм-дд): ");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime date))
        {
            Console.WriteLine("Невірний формат дати!");
            Console.ReadKey();
            return;
        }
        
        var matches = await _matchRepository.GetMatchesByDateRangeAsync(date);
        
        if (matches.Count == 0)
        {
            Console.WriteLine($"\nМатчів за {date:dd.MM.yyyy} не знайдено.");
        }
        else
        {
            Console.WriteLine($"\nМатчі за {date:dd.MM.yyyy}:");
            foreach (var match in matches)
            {
                Console.WriteLine($"\nID: {match.MatchId}");
                Console.WriteLine($"Час: {match.GameDate:HH:mm}");
                Console.WriteLine($"Сезон: {match.Season}");
                Console.WriteLine($"Тип: {match.MatchType}");
                Console.WriteLine($"Рахунок: {match.HomeTeamScore}:{match.AwayTeamScore}");
            }
        }
        
        Console.WriteLine($"\nЗнайдено матчів: {matches.Count}");
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task ShowMatchesForDateRange()
    {
        Console.Clear();
        Console.WriteLine("МАТЧІ ЗА ПЕРІОД");
        
        Console.Write("Введіть початкову дату (рррр-мм-дд): ");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime startDate))
        {
            Console.WriteLine("Невірний формат дати!");
            Console.ReadKey();
            return;
        }
        
        Console.Write("Введіть кінцеву дату (рррр-мм-дд): ");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime endDate))
        {
            Console.WriteLine("Невірний формат дати!");
            Console.ReadKey();
            return;
        }
        
        var matches = await _matchRepository.GetMatchesByDateRangeAsync(startDate, endDate);
        
        if (matches.Count == 0)
        {
            Console.WriteLine($"\nМатчів за період {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy} не знайдено.");
        }
        else
        {
            Console.WriteLine($"\nМатчі за період {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}:");
            foreach (var match in matches)
            {
                Console.WriteLine($"\nID: {match.MatchId}");
                Console.WriteLine($"Дата: {match.GameDate:dd.MM.yyyy HH:mm}");
                Console.WriteLine($"Сезон: {match.Season}");
                Console.WriteLine($"Тип: {match.MatchType}");
                Console.WriteLine($"Рахунок: {match.HomeTeamScore}:{match.AwayTeamScore}");
            }
        }
        
        Console.WriteLine($"\nЗнайдено матчів: {matches.Count}");
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task ManageStatistics()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("УПРАВЛІННЯ СТАТИСТИКОЮ");
            
            Console.WriteLine("1. Переглянути всю статистику");
            Console.WriteLine("2. Пошук статистики за ID");
            
            if (_currentUser?.UserRole == "Analyst")
            {
                Console.WriteLine("3. Назад");
            }
            else
            {
                Console.WriteLine("3. Додати нову статистику");
                Console.WriteLine("4. Редагувати статистику");
                Console.WriteLine("5. Видалити статистику (Soft Delete)");
                Console.WriteLine("6. Топ-гравці за очками");
                Console.WriteLine("7. Назад");
            }
            
            Console.Write("\nОберіть опцію: ");
            var choice = Console.ReadLine();
            
            if (_currentUser?.UserRole == "Analyst")
            {
                switch (choice)
                {
                    case "1":
                        await ShowRecentStatistics();
                        break;
                    case "2":
                        await SearchStatistic();
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Невірний вибір!");
                        Console.ReadKey();
                        break;
                }
            }
            else
            {
                switch (choice)
                {
                    case "1":
                        await ShowRecentStatistics();
                        break;
                    case "2":
                        await SearchStatistic();
                        break;
                    case "3":
                        await AddStatistic();
                        break;
                    case "4":
                        await EditStatistic();
                        break;
                    case "5":
                        await DeleteStatistic();
                        break;
                    case "6":
                        await ShowTopScorers();
                        break;
                    case "7":
                        return;
                    default:
                        Console.WriteLine("Невірний вибір!");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }

    static async Task ShowRecentStatistics()
    {
        Console.Clear();
        Console.WriteLine("ОСТАННЯ СТАТИСТИКА");
        
        var recentStats = await _statisticRepository.GetAllAsync();
        
        if (!recentStats.Any())
        {
            Console.WriteLine("Статистика відсутня.");
            Console.ReadKey();
            return;
        }
        
        Console.WriteLine($"{"ID",-5} {"Гравець",-20} {"Матч",-10} {"Очки",-8} {"Підб.",-8} {"Ас.",-8}");
        Console.WriteLine(new string('-', 60));
        
        foreach (var stat in recentStats.Take(20))
        {
            string playerName = stat.Player != null ? 
                $"{stat.Player.FirstName} {stat.Player.LastName}".Substring(0, Math.Min(18, $"{stat.Player.FirstName} {stat.Player.LastName}".Length)) : 
                "Невідомо";
            
            Console.WriteLine($"{stat.StatsId,-5} {playerName,-20} {stat.MatchId,-10} " +
                             $"{stat.Points ?? 0,-8} {stat.Rebounds ?? 0,-8} {stat.Assists ?? 0,-8}");
        }
        
        Console.WriteLine($"\nПоказано {Math.Min(recentStats.Count, 20)} записів з {recentStats.Count}");
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task SearchStatistic()
    {
        Console.Clear();
        PrintHeader(" ПОШУК СТАТИСТИКИ ЗА ID");
        
        Console.Write("Введіть ID статистики: ");
        if (!int.TryParse(Console.ReadLine(), out int statId))
        {
            PrintError("Невірний ID!");
            Console.ReadKey();
            return;
        }
        
        var statistic = await _statisticRepository.GetByIdAsync(statId);
        
        if (statistic == null)
        {
            PrintError("Статистика не знайдена!");
        }
        else
        {
            Console.WriteLine($"\nID: {statistic.StatsId}");
            Console.WriteLine($"Гравець: {statistic.Player?.FirstName} {statistic.Player?.LastName}");
            Console.WriteLine($"Матч ID: {statistic.MatchId}");
            Console.WriteLine($"Очки: {statistic.Points}");
            Console.WriteLine($"Підбирання: {statistic.Rebounds}");
            Console.WriteLine($"Асисти: {statistic.Assists}");
            Console.WriteLine($"Хвилини на полі: {statistic.MinutesPlayed}");
            Console.WriteLine($"Перехоплення: {statistic.Steals}");
            Console.WriteLine($"Блокшоти: {statistic.Blocks}");
            Console.WriteLine($"Втрати: {statistic.Turnovers}");
        }
        
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task AddStatistic()
{
    try
    {
        var statistic = await _statisticInputService.ReadStatisticDataAsync();
        
        var created = await _statisticTransactionService.CreateStatisticAsync(statistic);
        
        PrintSuccess($"Статистику з ID {created.StatsId} успішно додано!");
        
        await ShowStatisticDetails(created.StatsId);
    }
    catch (ValidationException ex)
    {
        PrintError($"Помилка валідації: {ex.Message}");
    }
    catch (TransactionException ex)
    {
        PrintError($"Помилка бази даних: {ex.Message}");
    }
    catch (Exception ex)
    {
        PrintError($"Неочікувана помилка: {ex.Message}");
    }
    
    Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
    Console.ReadKey();
}

    static async Task<int> GetIntInput(string fieldName, bool allowNull)
    {
        while (true)
        {
            Console.Write($"{fieldName} (Enter для {(allowNull ? "пропуску" : "0")}): ");
            var input = Console.ReadLine();
            
            if (string.IsNullOrEmpty(input))
            {
                return 0;
            }
            
            if (int.TryParse(input, out int result))
            {
                return result;
            }
            
            Console.WriteLine("Введіть число!");
        }
    }

    static async Task EditStatistic()
{
    Console.Clear();
    PrintHeader(" РЕДАГУВАННЯ СТАТИСТИКИ");
    
    try
    {
        Console.Write("Введіть ID статистики для редагування: ");
        if (!int.TryParse(Console.ReadLine(), out int statId))
        {
            PrintError("Невірний ID!");
            Console.ReadKey();
            return;
        }
        
        var statistic = await _context.Statistics
            .IgnoreQueryFilters()
            .Include(s => s.Player)
            .Include(s => s.Match)
            .FirstOrDefaultAsync(s => s.StatsId == statId);
        
        if (statistic == null)
        {
            PrintError($"Статистика з ID {statId} не знайдена!");
            Console.ReadKey();
            return;
        }
        
        Console.WriteLine("\nПОТОЧНІ ДАНІ:");
        Console.WriteLine($"Гравець: {statistic.Player?.FirstName} {statistic.Player?.LastName}");
        Console.WriteLine($"Матч: {statistic.Match?.GameDate:dd.MM.yyyy}");
        Console.WriteLine($"Очки: {statistic.Points ?? 0}");
        Console.WriteLine($"Підбирання: {statistic.Rebounds ?? 0}");
        Console.WriteLine($"Асисти: {statistic.Assists ?? 0}");
        
        Console.WriteLine("\nВВЕДІТЬ НОВІ ЗНАЧЕННЯ (Enter - залишити поточне):");
        
        Console.Write($"Очки ({statistic.Points ?? 0}): ");
        var pointsInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(pointsInput) && int.TryParse(pointsInput, out int newPoints))
            statistic.Points = newPoints;
        
        Console.Write($"Підбирання ({statistic.Rebounds ?? 0}): ");
        var reboundsInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(reboundsInput) && int.TryParse(reboundsInput, out int newRebounds))
            statistic.Rebounds = newRebounds;
        
        Console.Write($"Асисти ({statistic.Assists ?? 0}): ");
        var assistsInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(assistsInput) && int.TryParse(assistsInput, out int newAssists))
            statistic.Assists = newAssists;
        
        var updated = await _statisticTransactionService.UpdateStatisticAsync(statistic);
        
        PrintSuccess("Статистику успішно оновлено!");
        await ShowStatisticDetails(updated.StatsId);
    }
    catch (ValidationException ex)
    {
        PrintError($"Помилка валідації: {ex.Message}");
    }
    catch (Exception ex)
    {
        PrintError($"Помилка: {ex.Message}");
    }
    
    Console.ReadKey();
}

    static int? GetUpdatedIntValue(int? currentValue, string? input)
    {
        if (!string.IsNullOrEmpty(input))
        {
            if (int.TryParse(input, out int newValue))
            {
                if (newValue >= 0)
                {
                    Console.WriteLine($"   Оновлено: {newValue}");
                    return newValue;
                }
                else
                {
                    Console.WriteLine($"   Невірне значення. Залишено: {currentValue ?? 0}");
                }
            }
            else
            {
                Console.WriteLine($"   Невірний формат. Залишено: {currentValue ?? 0}");
            }
        }
        else
        {
            Console.WriteLine($"   Залишено без змін: {currentValue ?? 0}");
        }
        
        return currentValue;
    }

    static async Task ShowStatisticDetails(int statsId)
{
    var statistic = await _context.Statistics
        .IgnoreQueryFilters()
        .Include(s => s.Player)
        .Include(s => s.Match)
        .FirstOrDefaultAsync(s => s.StatsId == statsId);
    
    if (statistic == null) return;
    
    Console.WriteLine("\n=== ДЕТАЛІ СТАТИСТИКИ ===");
    Console.WriteLine($"ID: {statistic.StatsId}");
    Console.WriteLine($"Гравець: {statistic.Player?.FirstName} {statistic.Player?.LastName} (ID: {statistic.PlayerId})");
    Console.WriteLine($"Матч: ID {statistic.MatchId} ({statistic.Match?.GameDate:dd.MM.yyyy})");
    Console.WriteLine($"\nПОКАЗНИКИ:");
    Console.WriteLine($"  Очки: {statistic.Points ?? 0}");
    Console.WriteLine($"  Підбирання: {statistic.Rebounds ?? 0}");
    Console.WriteLine($"  Асисти: {statistic.Assists ?? 0}");
    Console.WriteLine($"  Перехоплення: {statistic.Steals ?? 0}");
    Console.WriteLine($"  Блокшоти: {statistic.Blocks ?? 0}");
    Console.WriteLine($"  Втрати: {statistic.Turnovers ?? 0}");
    
    if (statistic.MinutesPlayed.HasValue)
    {
        Console.WriteLine($"  Хвилини на полі: {statistic.MinutesPlayed:HH:mm}");
    }
}

    static async Task DeleteStatistic()
{
    if (_currentUser?.UserRole != "Admin")
    {
        PrintError("Доступ заборонено! Видалення тільки для адміністраторів.");
        Console.ReadKey();
        return;
    }
    
    Console.Clear();
    PrintHeader(" ВИДАЛЕННЯ СТАТИСТИКИ (SOFT DELETE)");
    
    Console.Write("Введіть ID статистики для видалення: ");
    if (!int.TryParse(Console.ReadLine(), out int statId))
    {
        PrintError("Невірний ID!");
        Console.ReadKey();
        return;
    }
    
    var statistic = await _context.Statistics
        .IgnoreQueryFilters()
        .Include(s => s.Player)
        .FirstOrDefaultAsync(s => s.StatsId == statId);
    
    if (statistic == null)
    {
        PrintError("Статистика не знайдена!");
        Console.ReadKey();
        return;
    }
    
    Console.WriteLine($"\nІнформація про статистику:");
    Console.WriteLine($"  Гравець: {statistic.Player?.FirstName} {statistic.Player?.LastName}");
    Console.WriteLine($"  Матч ID: {statistic.MatchId}");
    Console.WriteLine($"  Очки: {statistic.Points ?? 0}");
    
    Console.Write($"\nВидалити статистику? (y/n): ");
    var confirm = Console.ReadLine();
    
    if (confirm?.ToLower() == "y")
    {
        try
        {
            bool success = await _statisticTransactionService.DeleteStatisticAsync(statId);
            
            if (success)
                PrintSuccess("Статистику успішно видалено (Soft Delete)!");
            else
                PrintError("Не вдалося видалити статистику!");
        }
        catch (Exception ex)
        {
            PrintError($"Помилка: {ex.Message}");
        }
    }
    else
    {
        PrintInfo("Видалення скасовано.");
    }
    
    Console.ReadKey();
}

    static async Task ShowTopScorers()
    {
        Console.Clear();
        PrintHeader(" ТОП-ГРАВЦІ ЗА ОЧКАМИ");
        
        Console.Write("Кількість гравців для відображення: ");
        if (!int.TryParse(Console.ReadLine(), out int topN) || topN <= 0)
            topN = 10;
        
        var topScorers = await _playerRepository.GetTopScorersAsync(topN);
        
        if (topScorers.Count == 0)
        {
            Console.WriteLine("Дані відсутні.");
        }
        else
        {
            Console.WriteLine($"\nТоп-{topN} гравців за очками:");
            int position = 1;
            foreach (var player in topScorers)
            {
                var stats = await _statisticRepository.GetPlayerStatisticsAsync(player.PlayerId);
                var totalPoints = stats.Sum(s => s.Points ?? 0);
                
                Console.WriteLine($"{position}. {player.FirstName} {player.LastName} - {totalPoints} очок");
                position++;
            }
        }
        
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task ShowReports()
    {
        while (true)
        {
            Console.Clear();
            PrintHeader(" ЗВІТИ ТА СКЛАДНІ ЗАПИТИ");
            
            Console.WriteLine("1. Статистика команд");
            Console.WriteLine("2. Команди з повними складами");
            Console.WriteLine("3. Гравці з пагінацією");
            Console.WriteLine("4. Транзакційні операції");
            Console.WriteLine("5. Назад");
            
            Console.Write("\nОберіть опцію: ");
            var choice = Console.ReadLine();
            
            var nbaService = new NbaService(_context);
            
            switch (choice)
            {
                case "1":
                    var teamStats = await nbaService.GetTeamStatsAsync();
                    Console.Clear();
                    PrintHeader("СТАТИСТИКА КОМАНД");
                    foreach (var stat in teamStats)
                    {
                        Console.WriteLine($"\nКоманда: {stat.TeamName}");
                        Console.WriteLine($"Кількість гравців: {stat.PlayerCount}");
                        Console.WriteLine($"Середній зріст: {stat.AverageHeight:F1} см");
                        Console.WriteLine($"Середні очки: {stat.AveragePointsScored:F1} ");
                        
                    }
                    Console.ReadKey();
                    break;
                    
                case "2":
                    var teamsWithRosters = await nbaService.GetTeamsWithRostersAsync();
                    Console.Clear();
                    Console.WriteLine("КОМАНДИ З ПОВНИМИ СКЛАДАМИ");
                    
                    if (!teamsWithRosters.Any())
                    {
                        Console.WriteLine("\nКоманди не знайдені.");
                        Console.ReadKey();
                        break;
                    }
                    
                    Console.WriteLine($"\nЗнайдено команд: {teamsWithRosters.Count}");
                    
                    foreach (var team in teamsWithRosters)
                    {
                        Console.WriteLine($"\n══════════════════════════════════════════════════════");
                        Console.WriteLine($" КОМАНДА: {team.TeamName.ToUpper()} ({team.Abbreviation})");
                        Console.WriteLine($" Арена: {team.Arena?.ArenaName ?? "Невідомо"}");
                        Console.WriteLine($" Рік заснування: {team.YearFounded?.Year.ToString() ?? "Невідомо"}");
                        
                        var activePlayers = team.Players?.Where(p => !p.IsDeleted).ToList() ?? new List<Player>();
                        var deletedPlayers = team.Players?.Where(p => p.IsDeleted).ToList() ?? new List<Player>();
                        
                        Console.WriteLine($" Активних гравців: {activePlayers.Count}");
                        if (deletedPlayers.Count > 0)
                        {
                            Console.WriteLine($"  Видалених гравців (Soft Delete): {deletedPlayers.Count}");
                        }
                        
                        if (activePlayers.Any())
                        {
                            
                            Console.WriteLine($"                     СКЛАД КОМАНДИ                      ");
                            Console.WriteLine($" {"№",-3} {"ID",-5}  {"Гравець",-22}  {"Позиція",-9} {"Країна",-7}");
                            
                            foreach (var player in activePlayers.OrderBy(p => p.JerseyNumber))
                            {
                                string playerName = $"{player.FirstName} {player.LastName}";
                                if (playerName.Length > 22)
                                    playerName = playerName.Substring(0, 19) + "...";
                                    
                                Console.WriteLine($" {player.JerseyNumber,-3} {player.PlayerId,-5} " +
                                                $" {playerName,-22}  {player.Position,-9} {player.Country,-7}");
                            }
                            
                            
                            var positionGroups = activePlayers
                                .GroupBy(p => p.Position)
                                .Select(g => new { Position = g.Key, Count = g.Count() })
                                .OrderByDescending(g => g.Count);
                            
                            Console.WriteLine($"\n РОЗПОДІЛ ПО ПОЗИЦІЯХ:");
                            Console.WriteLine($" {"Позиція",-15}│ {"Гравців",-7} ");
                            
                            foreach (var group in positionGroups)
                            {
                                Console.WriteLine($" {group.Position,-15} {group.Count,-7}");
                            }
                            
                            var averageAge = DateTime.Now.Year - activePlayers.Average(p => p.BirthDate.Year);
                            var averageHeight = activePlayers.Average(p => p.HeightCm);
                            var averageWeight = activePlayers.Average(p => p.WeightKg);
                            
                            
                            Console.WriteLine($"\n СЕРЕДНІ ПОКАЗНИКИ ГРАВЦІВ:");
                            Console.WriteLine($" {"Середній вік:",-20} {averageAge:F1} років ");
                            Console.WriteLine($" {"Середній зріст:",-20} {averageHeight:F1} см   ");
                            Console.WriteLine($" {"Середня вага:",-20} {averageWeight:F1} кг   ");
                           
                        }
                        else
                        {
                            Console.WriteLine($"\n  У команді ще немає активних гравців");
                            
                            if (deletedPlayers.Any() && _currentUser?.UserRole == "Admin")
                            {
                                Console.WriteLine($"\n  ВИДАЛЕНІ ГРАВЦІ (Soft Delete):");
                                foreach (var player in deletedPlayers.Take(5))
                                {
                                    Console.WriteLine($"  {player.JerseyNumber}. {player.FirstName} {player.LastName}");
                                }
                                if (deletedPlayers.Count > 5)
                                    Console.WriteLine($"  ... та ще {deletedPlayers.Count - 5} гравців");
                            }
                        }
                        
                        if (team.Coaches?.Any() == true)
                        {
                            Console.WriteLine($"\n ТРЕНЕРСЬКИЙ ШТАБ ({team.Coaches.Count} осіб):");
                            Console.WriteLine($" {"Ім'я",-18} {"Прізвище",-18} {"Посада",-16}");
                            
                            foreach (var coach in team.Coaches)
                            {
                                Console.WriteLine($"│ {coach.FirstName,-18}│ {coach.LastName,-18}│ {coach.Role,-16}│");
                            }
                            
                            
                        }
                        
                        if (deletedPlayers.Any() && _currentUser?.UserRole == "Admin")
                        {
                            Console.WriteLine($"\n ДЛЯ АДМІНІСТРАТОРА:");
                            Console.WriteLine($"  Видалених гравців: {deletedPlayers.Count}");
                            Console.Write($"   Показати детальну інформацію? (y/n): ");
                            
                            if (Console.ReadLine()?.ToLower() == "y")
                            {
                                Console.WriteLine($"\n  ДЕТАЛЬНИЙ СПИСОК ВИДАЛЕНИХ ГРАВЦІВ:");
                                Console.WriteLine($"   {"ID",-6} {"Гравець",-25} {"Позиція",-12} {"Причина"}");
                                Console.WriteLine($"   {new string('─', 70)}");
                                
                                foreach (var player in deletedPlayers.OrderBy(p => p.LastName))
                                {
                                    Console.WriteLine($"   {player.PlayerId,-6} " +
                                                    $"{player.FirstName} {player.LastName,-18} " +
                                                    $"{player.Position,-12} {"Soft Delete"}");
                                }
                            }
                        }
                        
                        Console.WriteLine($"\n══════════════════════════════════════════════════════");
                        Console.WriteLine($"Натисніть будь-яку клавішу для наступної команди...");
                        Console.ReadKey();
                    }
                    Console.WriteLine($"\nНатисніть будь-яку клавішу для продовження...");
                    Console.ReadKey();
                    break;
                    
                case "3":
                    Console.Write("Сторінка: ");
                    if (!int.TryParse(Console.ReadLine(), out int page))
                        page = 1;
                    
                    Console.Write("Розмір сторінки: ");
                    if (!int.TryParse(Console.ReadLine(), out int pageSize))
                        pageSize = 10;
                    
                    Console.Write("Фільтр по команді (Enter для пропуску): ");
                    var filter = Console.ReadLine();
                    
                    var players = await nbaService.GetPlayersPagedAsync(page, pageSize, filter);
                    Console.Clear();
                    PrintHeader(" ГРАВЦІ З ПАГІНАЦІЄЮ");
                    foreach (var player in players)
                    {
                        Console.WriteLine($"{player.FirstName} {player.LastName} - {player.Position}");
                    }
                    Console.WriteLine($"\nСторінка {page}, показано {players.Count} гравців");
                    Console.ReadKey();
                    break;
                    
                    
                case "4":
                    await TestTransactions();
                    break;
                    
                case "5":
                    return;
            }
        }
    }

    static async Task TestTransactions()
    {
        if (_currentUser?.UserRole == "Analyst")
        {
            Console.WriteLine("Доступ заборонено! Аналіст може тільки переглядати дані.");
            Console.ReadKey();
            return;
        }
        
        Console.Clear();
        PrintHeader(" ТЕСТ ТРАНЗАКЦІЙНИХ ОПЕРАЦІЙ");
        
        var nbaService = new NbaService(_context);
        
        Console.WriteLine("1. Трейд гравця між командами");
        Console.WriteLine("2. Оновлення місткості арени з ADO.NET");
        Console.WriteLine("3. Назад");
        
        Console.Write("\nОберіть опцію: ");
        var choice = Console.ReadLine();
        
        switch (choice)
        {
            case "1":
                Console.Write("ID гравця: ");
                if (!int.TryParse(Console.ReadLine(), out int playerId))
                {
                    PrintError("Невірний ID!");
                    break;
                }
                
                Console.Write("ID нової команди: ");
                if (!int.TryParse(Console.ReadLine(), out int newTeamId))
                {
                    PrintError("Невірний ID команди!");
                    break;
                }
                
                try
                {
                    await nbaService.TradePlayerAsync(playerId, newTeamId);
                    PrintSuccess("Транзакція виконана успішно!");
                }
                catch (Exception ex)
                {
                    PrintError($"Помилка транзакції: {ex.Message}");
                }
                break;
                
            case "2":
                Console.Write("ID арени: ");
                if (!int.TryParse(Console.ReadLine(), out int arenaId))
                {
                    PrintError("Невірний ID!");
                    break;
                }
                
                Console.Write("Нова місткість: ");
                if (!int.TryParse(Console.ReadLine(), out int newCapacity))
                {
                    PrintError("Невірна місткість!");
                    break;
                }
                
                try
                {
                    await nbaService.UpdateArenaCapacityAdoNetAsync(arenaId, newCapacity);
                    PrintSuccess("Змішана транзакція (ADO.NET + EF) виконана успішно!");
                }
                catch (Exception ex)
                {
                    PrintError($"Помилка транзакції: {ex.Message}");
                }
                break;
        }
        
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task AdminDeletedItemsMenu()
    {
        while (true)
        {
            Console.Clear();
            PrintHeader(" АДМІН-ПАНЕЛЬ: УПРАВЛІННЯ ВИДАЛЕНИМИ ЗАПИСАМИ");
            
            Console.WriteLine("1. Переглянути видалені команди");
            Console.WriteLine("2. Переглянути видалених гравців");
            Console.WriteLine("3. Переглянути видалених тренерів");
            Console.WriteLine("4. Переглянути видалені матчі");
            Console.WriteLine("5. Переглянути видалену статистику");
            Console.WriteLine("6. Відновити видалений запис");
            Console.WriteLine("7. Повне видалення (hard delete)");
            Console.WriteLine("8. Назад");
            
            Console.Write("\nОберіть опцію: ");
            var choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    await ShowDeletedTeams();
                    break;
                case "2":
                    await ShowDeletedPlayers();
                    break;
                case "3":
                    await ShowDeletedCoaches();
                    break;
                case "4":
                    await ShowDeletedMatches();
                    break;
                case "5":
                    await ShowDeletedStatistics();
                    break;
                case "6":
                    await RestoreDeletedItem();
                    break;
                case "7":
                    await HardDeleteItem();
                    break;
                case "8":
                    return;
                default:
                    Console.WriteLine("Невірний вибір!");
                    Console.ReadKey();
                    break;
            }
        }
    }

    static async Task ShowDeletedTeams()
    {
        Console.Clear();
        PrintHeader(" ВИДАЛЕНІ КОМАНДИ (SOFT DELETE)");
        
        var deletedTeams = await _teamRepository.GetDeletedTeamsAsync();
        
        if (!deletedTeams.Any())
        {
            Console.WriteLine("Видалених команд не знайдено.");
        }
        else
        {
            Console.WriteLine($"{"ID",-6} {"Назва команди",-25} {"Арена",-20} {"Дивізіон",-15}");
            Console.WriteLine(new string('─', 70));
            
            foreach (var team in deletedTeams)
            {
                string teamName = team.TeamName.Length > 23 ? team.TeamName.Substring(0, 20) + "..." : team.TeamName;
                string arenaName = team.Arena?.ArenaName ?? "Немає";
                arenaName = arenaName.Length > 18 ? arenaName.Substring(0, 15) + "..." : arenaName;
                string divisionName = team.Division?.DivisionName ?? "Немає";
                
                Console.WriteLine($"{team.TeamId,-6} {teamName,-25} {arenaName,-20} {divisionName,-15}");
            }
        }
        
        Console.WriteLine($"\nВсього видалених команд: {deletedTeams.Count}");
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task ShowDeletedPlayers()
    {
        Console.Clear();
        PrintHeader(" ВИДАЛЕНІ ГРАВЦІ (SOFT DELETE)");
        
        var deletedPlayers = await _playerRepository.GetDeletedPlayersAsync();
        
        if (!deletedPlayers.Any())
        {
            Console.WriteLine("Видалених гравців не знайдено.");
        }
        else
        {
            Console.WriteLine($"{"ID",-6} {"Гравець",-25} {"Команда",-20} {"Позиція",-12} {"Країна",-15}");
            Console.WriteLine(new string('─', 80));
            
            foreach (var player in deletedPlayers)
            {
                string playerName = $"{player.FirstName} {player.LastName}";
                if (playerName.Length > 22)
                    playerName = playerName.Substring(0, 19) + "...";
                    
                string teamName = player.Team?.TeamName ?? "Без команди";
                if (teamName.Length > 18)
                    teamName = teamName.Substring(0, 15) + "...";
                
                Console.WriteLine($"{player.PlayerId,-6} {playerName,-25} {teamName,-20} " +
                                 $"{player.Position,-12} {player.Country,-15}");
            }
        }
        
        Console.WriteLine($"\nВсього видалених гравців: {deletedPlayers.Count}");
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task ShowDeletedCoaches()
    {
        Console.Clear();
        PrintHeader(" ВИДАЛЕНІ ТРЕНЕРИ (SOFT DELETE)");
        
        var deletedCoaches = await _coachRepository.GetDeletedCoachesAsync();
        
        if (!deletedCoaches.Any())
        {
            Console.WriteLine("Видалених тренерів не знайдено.");
        }
        else
        {
            Console.WriteLine($"{"ID",-6} {"Тренер",-25} {"Команда",-20} {"Посада",-15}");
            Console.WriteLine(new string('─', 70));
            
            foreach (var coach in deletedCoaches)
            {
                string coachName = $"{coach.FirstName} {coach.LastName}";
                if (coachName.Length > 22)
                    coachName = coachName.Substring(0, 19) + "...";
                    
                string teamName = coach.Team?.TeamName ?? "Без команди";
                if (teamName.Length > 18)
                    teamName = teamName.Substring(0, 15) + "...";
                
                Console.WriteLine($"{coach.CoachId,-6} {coachName,-25} {teamName,-20} {coach.Role,-15}");
            }
        }
        
        Console.WriteLine($"\nВсього видалених тренерів: {deletedCoaches.Count}");
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task ShowDeletedMatches()
    {
        Console.Clear();
        PrintHeader(" ВИДАЛЕНІ МАТЧІ (SOFT DELETE)");
        
        var deletedMatches = await _matchRepository.GetDeletedMatchesAsync();
        
        if (!deletedMatches.Any())
        {
            Console.WriteLine("Видалених матчів не знайдено.");
        }
        else
        {
            Console.WriteLine($"{"ID",-6} {"Дата",-12} {"Сезон",-12} {"Тип",-10} {"Рахунок",-15}");
            Console.WriteLine(new string('─', 60));
            
            foreach (var match in deletedMatches)
            {
                string score = $"{match.HomeTeamScore}:{match.AwayTeamScore}";
                Console.WriteLine($"{match.MatchId,-6} {match.GameDate:dd.MM.yyyy,-12} " +
                                 $"{match.Season,-12} {match.MatchType,-10} {score,-15}");
            }
        }
        
        Console.WriteLine($"\nВсього видалених матчів: {deletedMatches.Count}");
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task ShowDeletedStatistics()
    {
        Console.Clear();
        PrintHeader(" ВИДАЛЕНА СТАТИСТИКА (SOFT DELETE)");
        
        var deletedStats = await _statisticRepository.GetDeletedStatisticsAsync();
        
        if (!deletedStats.Any())
        {
            Console.WriteLine("Видаленої статистики не знайдено.");
        }
        else
        {
            Console.WriteLine($"{"ID",-6} {"Гравець",-20} {"Матч ID",-10} {"Очки",-8} {"Підб.",-8} {"Ас.",-8}");
            Console.WriteLine(new string('─', 60));
            
            foreach (var stat in deletedStats)
            {
                string playerName = stat.Player != null ? 
                    $"{stat.Player.FirstName} {stat.Player.LastName}" : 
                    "Невідомо";
                
                if (playerName.Length > 18)
                    playerName = playerName.Substring(0, 15) + "...";
                
                Console.WriteLine($"{stat.StatsId,-6} {playerName,-20} {stat.MatchId,-10} " +
                                 $"{stat.Points ?? 0,-8} {stat.Rebounds ?? 0,-8} {stat.Assists ?? 0,-8}");
            }
        }
        
        Console.WriteLine($"\nВсього видаленої статистики: {deletedStats.Count}");
        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
        Console.ReadKey();
    }

    static async Task RestoreDeletedItem()
    {
        Console.Clear();
        PrintHeader(" ВІДНОВЛЕННЯ ВИДАЛЕНОГО ЗАПИСУ");
        
        Console.WriteLine("Оберіть тип запису для відновлення:");
        Console.WriteLine("1. Команда");
        Console.WriteLine("2. Гравець");
        Console.WriteLine("3. Тренер");
        Console.WriteLine("4. Матч");
        Console.WriteLine("5. Статистика");
        Console.WriteLine("6. Назад");
        
        Console.Write("\nОберіть опцію: ");
        var typeChoice = Console.ReadLine();
        
        if (typeChoice == "6") return;
        
        Console.Write("Введіть ID запису для відновлення: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            PrintError("Невірний ID!");
            Console.ReadKey();
            return;
        }
        
        bool success = false;
        string entityName = "";
        
        try
        {
            switch (typeChoice)
            {
                case "1":
                    success = await _teamRepository.RestoreAsync(id);
                    entityName = "команду";
                    break;
                case "2":
                    success = await _playerRepository.RestoreAsync(id);
                    entityName = "гравця";
                    break;
                case "3":
                    success = await _coachRepository.RestoreAsync(id);
                    entityName = "тренера";
                    break;
                case "4":
                    success = await _matchRepository.RestoreAsync(id);
                    entityName = "матч";
                    break;
                case "5":
                    success = await _statisticRepository.RestoreAsync(id);
                    entityName = "статистику";
                    break;
                default:
                    PrintError("Невірний вибір типу!");
                    Console.ReadKey();
                    return;
            }
            
            if (success)
            {
                PrintSuccess($"{entityName} з ID {id} успішно відновлено!");
            }
            else
            {
                PrintError($"Не вдалося відновити {entityName} з ID {id}. " +
                          "Можливо, запис не знайдений або не був видалений.");
            }
        }
        catch (Exception ex)
        {
            PrintError($"Помилка при відновленні: {ex.Message}");
        }
        
        Console.ReadKey();
    }

    static async Task HardDeleteItem()
    {
        Console.Clear();
        PrintHeader(" ПОВНЕ ВИДАЛЕННЯ (HARD DELETE)");
        
        
        Console.WriteLine("Оберіть тип запису для повного видалення:");
        Console.WriteLine("1. Гравець");
        Console.WriteLine("2. Назад");
        
        Console.Write("\nОберіть опцію: ");
        var typeChoice = Console.ReadLine();
        
        if (typeChoice == "2") return;
        
        Console.Write("Введіть ID запису для повного видалення: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            PrintError("Невірний ID!");
            Console.ReadKey();
            return;
        }
        
        Console.Write($"\nВи ТОЧНО впевнені, що хочете ПОВНІСТЮ видалити запис з ID {id}? (yes/no): ");
        var confirm = Console.ReadLine()?.ToLower();
        
        if (confirm != "yes" && confirm != "y")
        {
            PrintInfo("Операція скасована.");
            Console.ReadKey();
            return;
        }
        
        bool success = false;
        string entityName = "";
        
        try
        {
            switch (typeChoice)
            {
                case "1":
                    success = await _playerRepository.HardDeleteAsync(id);
                    entityName = "гравця";
                    break;
                default:
                    PrintError("Невірний вибір типу!");
                    Console.ReadKey();
                    return;
            }
            
            if (success)
            {
                PrintSuccess($"{entityName} з ID {id} повністю видалено з бази даних!");
            }
            else
            {
                PrintError($"Не вдалося видалити {entityName} з ID {id}.");
            }
        }
        catch (Exception ex)
        {
            PrintError($"Помилка при видаленні: {ex.Message}");
        }
        
        Console.ReadKey();
    }

    static string ReadPassword()
    {
        var password = "";
        ConsoleKeyInfo key;
        
        do
        {
            key = Console.ReadKey(true);
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[0..^1];
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);
        
        Console.WriteLine();
        return password;
    }

    static void PrintHeader(string text) 
    { 
        Console.ForegroundColor = ConsoleColor.Cyan; 
        Console.WriteLine($"\n{text}");
        Console.WriteLine(new string('=', text.Length));
        Console.ResetColor(); 
    }

    static void PrintSuccess(string text) 
    { 
        Console.ForegroundColor = ConsoleColor.Green; 
        Console.WriteLine($"[✓] {text}"); 
        Console.ResetColor(); 
    }

    static void PrintError(string text) 
    { 
        Console.ForegroundColor = ConsoleColor.Red; 
        Console.WriteLine($"[✗] {text}"); 
        Console.ResetColor(); 
    }

    static void PrintInfo(string text) 
    { 
        Console.ForegroundColor = ConsoleColor.Yellow; 
        Console.WriteLine($"[!] {text}"); 
        Console.ResetColor(); 
    }
}
