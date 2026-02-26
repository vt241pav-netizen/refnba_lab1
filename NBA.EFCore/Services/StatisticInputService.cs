
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NBA.EFCore.Data;
using NBA.EFCore.EFModels;
using NBA.EFCore.Exceptions;

namespace NBA.EFCore.Services
{

    public class StatisticInputService
    {
        private readonly NbaDbContext _context;
        
        public StatisticInputService(NbaDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        

        public async Task<Statistic> ReadStatisticDataAsync()
        {
            Console.Clear();
            PrintHeader(" ДОДАВАННЯ СТАТИСТИКИ");
            
            int statsId = await ReadPositiveIntAsync("ID статистики: ");
            await ValidateStatisticIdUniqueAsync(statsId);
            
            int matchId = await ReadPositiveIntAsync("ID матчу: ");
            var match = await ValidateMatchExistsAsync(matchId);
            
            int playerId = await ReadPositiveIntAsync("ID гравця: ");
            var player = await ValidatePlayerExistsAsync(playerId);
            
            await ValidatePlayerInMatchAsync(player, match);
            
            await ValidatePlayerStatNotDuplicateAsync(playerId, matchId);
            
            Console.WriteLine("\n--- ВВЕДІТЬ СТАТИСТИЧНІ ПОКАЗНИКИ ---");
            
            int points = await ReadIntAsync("Очки: ");
            int rebounds = await ReadIntAsync("Підбирання: ");
            int assists = await ReadIntAsync("Асисти: ");
            int steals = await ReadIntAsync("Перехоплення: ");
            int blocks = await ReadIntAsync("Блокшоти: ");
            int turnovers = await ReadIntAsync("Втрати: ");
            int minutes = await ReadIntAsync("Хвилини на полі: ");
            
            Console.WriteLine("\n--- ПЕРЕВІРТЕ ВВЕДЕНІ ДАНІ ---");
            Console.WriteLine($"ID статистики: {statsId}");
            Console.WriteLine($"ID матчу: {matchId} (Дата: {match.GameDate:dd.MM.yyyy})");
            Console.WriteLine($"ID гравця: {playerId} (Гравець: {player.FirstName} {player.LastName})");
            Console.WriteLine($"Очки: {points}");
            Console.WriteLine($"Підбирання: {rebounds}");
            Console.WriteLine($"Асисти: {assists}");
            Console.WriteLine($"Перехоплення: {steals}");
            Console.WriteLine($"Блокшоти: {blocks}");
            Console.WriteLine($"Втрати: {turnovers}");
            Console.WriteLine($"Хвилини: {minutes}");
            
            Console.Write("\nЗберегти статистику? (y/n): ");
            if (Console.ReadLine()?.ToLower() != "y")
            {
                throw new ValidationException("Операцію скасовано користувачем");
            }
            
            return new Statistic
            {
                StatsId = statsId,
                MatchId = matchId,
                PlayerId = playerId,
                Points = points,
                Rebounds = rebounds,
                Assists = assists,
                Steals = steals,
                Blocks = blocks,
                Turnovers = turnovers,
                MinutesPlayed = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(minutes)),
                IsDeleted = false
            };
        }
        

        private async Task<int> ReadPositiveIntAsync(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(input))
                {
                    PrintError("Значення не може бути порожнім!");
                    continue;
                }
                
                if (int.TryParse(input, out int result))
                {
                    if (result > 0)
                    {
                        return result;
                    }
                    PrintError("Число має бути більше 0!");
                }
                else
                {
                    PrintError("Введіть коректне число!");
                }
            }
        }

        private async Task<int> ReadIntAsync(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(input))
                {
                    PrintError("Значення не може бути порожнім!");
                    continue;
                }
                
                if (int.TryParse(input, out int result))
                {
                    if (result >= 0)
                    {
                        return result;
                    }
                    PrintError("Число має бути більше або дорівнювати 0!");
                }
                else
                {
                    PrintError("Введіть коректне число!");
                }
            }
        }

        private async Task ValidateStatisticIdUniqueAsync(int statsId)
        {
            bool exists = await _context.Statistics
                .IgnoreQueryFilters()
                .AnyAsync(s => s.StatsId == statsId);
                
            if (exists)
            {
                throw new ValidationException($"Статистика з ID {statsId} вже існує в базі даних");
            }
        }
        

        private async Task<Match> ValidateMatchExistsAsync(int matchId)
        {
            var match = await _context.Matches
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.MatchId == matchId);
                
            if (match == null)
            {
                throw new ValidationException($"Матч з ID {matchId} не знайдений в базі даних");
            }
            
            return match;
        }
        

        private async Task<Player> ValidatePlayerExistsAsync(int playerId)
        {
            var player = await _context.Players
                .IgnoreQueryFilters()
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.PlayerId == playerId);
                
            if (player == null)
            {
                throw new ValidationException($"Гравець з ID {playerId} не знайдений в базі даних");
            }
            
            return player;
        }
        

        private async Task ValidatePlayerInMatchAsync(Player player, Match match)
        {
            if (player.TeamId != match.HomeTeamId && player.TeamId != match.AwayTeamId)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n⚠️ УВАГА: Гравець {player.FirstName} {player.LastName} (команда ID: {player.TeamId})");
                Console.WriteLine($"не брав участі в матчі {match.MatchId} (команди: {match.HomeTeamId} vs {match.AwayTeamId})");
                Console.ResetColor();
                
                Console.Write("Все одно додати статистику? (y/n): ");
                if (Console.ReadLine()?.ToLower() != "y")
                {
                    throw new ValidationException("Операцію скасовано користувачем");
                }
            }
        }
        

        private async Task ValidatePlayerStatNotDuplicateAsync(int playerId, int matchId)
        {
            bool exists = await _context.Statistics
                .IgnoreQueryFilters()
                .AnyAsync(s => s.PlayerId == playerId && s.MatchId == matchId);
                
            if (exists)
            {
                throw new ValidationException($"Статистика для гравця {playerId} у матчі {matchId} вже існує");
            }
        }
        
        private void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ {message}");
            Console.ResetColor();
        }
        

        private void PrintHeader(string text)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n{text}");
            Console.WriteLine(new string('=', text.Length));
            Console.ResetColor();
        }
    }
}