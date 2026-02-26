using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NBA.EFCore.Constants;

namespace NBA.EFCore.Services
{

    public class MenuService
    {

        public async Task ShowManagementMenu(
            string title,
            Dictionary<string, Func<Task>> options,
            string userRole)
        {
            while (true)
            {
                Console.Clear();
                
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n{title} ({userRole})");
                Console.WriteLine(new string('=', title.Length + userRole.Length + 3));
                Console.ResetColor();
                
                int i = 1;
                var optionList = options.ToList();
                
                foreach (var option in optionList)
                {
                    Console.WriteLine($"{i++}. {option.Key}");
                }
                
                Console.Write("\nОберіть опцію: ");
                var choice = Console.ReadLine();
                
                if (int.TryParse(choice, out int index) && 
                    index > 0 && 
                    index <= optionList.Count)
                {
                    var selectedOption = optionList[index - 1];
                    
                    if (selectedOption.Key == "Назад" || selectedOption.Value == null)
                    {
                        break;
                    }
                    
                    await selectedOption.Value();
                }
                else
                {
                    Console.WriteLine("Невірний вибір!");
                    Console.ReadKey();
                }
            }
        }
        
        public Dictionary<string, Func<Task>> CreateBaseMenu(
            Func<Task> viewAll,
            Func<Task> searchById,
            Func<Task>? add = null,
            Func<Task>? edit = null,
            Func<Task>? delete = null,
            params (string Title, Func<Task> Action)[] additionalOptions)
        {
            var menu = new Dictionary<string, Func<Task>>
            {
                ["Переглянути всі"] = viewAll,
                ["Пошук за ID"] = searchById
            };
            
            if (add != null)
                menu["Додати новий"] = add;
                
            if (edit != null)
                menu["Редагувати"] = edit;
                
            if (delete != null)
                menu["Видалити"] = delete;
            
            foreach (var option in additionalOptions)
            {
                if (!string.IsNullOrEmpty(option.Title) && option.Action != null)
                {
                    menu[option.Title] = option.Action;
                }
            }
            
            menu["Назад"] = null;
            
            return menu;
        }
        
        public Dictionary<string, Func<Task>> CreateReportsMenu(
            Func<Task> teamStats,
            Func<Task> teamsWithRosters,
            Func<Task> playersPaged,
            Func<Task> transactions)
        {
            return new Dictionary<string, Func<Task>>
            {
                ["Статистика команд"] = teamStats,
                ["Команди з повними складами"] = teamsWithRosters,
                ["Гравці з пагінацією"] = playersPaged,
                ["Транзакційні операції"] = transactions,
                ["Назад"] = null
            };
        }
        
        public Dictionary<string, Func<Task>> CreateAdminMenu(
            Func<Task> deletedTeams,
            Func<Task> deletedPlayers,
            Func<Task> deletedCoaches,
            Func<Task> deletedMatches,
            Func<Task> deletedStats,
            Func<Task> restoreItem,
            Func<Task> hardDelete)
        {
            return new Dictionary<string, Func<Task>>
            {
                ["Переглянути видалені команди"] = deletedTeams,
                ["Переглянути видалених гравців"] = deletedPlayers,
                ["Переглянути видалених тренерів"] = deletedCoaches,
                ["Переглянути видалені матчі"] = deletedMatches,
                ["Переглянути видалену статистику"] = deletedStats,
                ["Відновити видалений запис"] = restoreItem,
                ["Повне видалення (hard delete)"] = hardDelete,
                ["Назад"] = null
            };
        }
    }
}