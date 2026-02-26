
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NBA.EFCore.Data;
using NBA.EFCore.EFModels;
using NBA.EFCore.Exceptions;

namespace NBA.EFCore.Services
{

    public class StatisticTransactionService
    {
        private readonly NbaDbContext _context;
        
        public StatisticTransactionService(NbaDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        
        public async Task<Statistic> CreateStatisticAsync(Statistic statistic)
        {
            if (statistic == null)
                throw new ArgumentNullException(nameof(statistic));
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                Console.WriteLine("\n Початок транзакції...");
                
                await _context.Statistics.AddAsync(statistic);
                int saved = await _context.SaveChangesAsync();
                
                Console.WriteLine($" Збережено {saved} запис(ів)");
                
                await transaction.CommitAsync();
                Console.WriteLine(" Транзакцію підтверджено (commit)");
                
                return statistic;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($" Транзакцію скасовано (rollback): {ex.Message}");
                
                throw new TransactionException("Помилка при збереженні статистики", ex);
            }
        }
        

        public async Task<Statistic> UpdateStatisticAsync(Statistic statistic)
        {
            if (statistic == null)
                throw new ArgumentNullException(nameof(statistic));
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var existingStat = await _context.Statistics
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.StatsId == statistic.StatsId);
                    
                if (existingStat == null)
                    throw new ValidationException($"Статистика з ID {statistic.StatsId} не знайдена");
                
                existingStat.Points = statistic.Points;
                existingStat.Rebounds = statistic.Rebounds;
                existingStat.Assists = statistic.Assists;
                existingStat.Steals = statistic.Steals;
                existingStat.Blocks = statistic.Blocks;
                existingStat.Turnovers = statistic.Turnovers;
                existingStat.MinutesPlayed = statistic.MinutesPlayed;
                
                _context.Statistics.Update(existingStat);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
                
                return existingStat;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        

        public async Task<bool> DeleteStatisticAsync(int statsId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var statistic = await _context.Statistics
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.StatsId == statsId);
                    
                if (statistic == null)
                    return false;
                    
                statistic.IsDeleted = true;
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
                
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        

        public async Task<List<Statistic>> CreateBulkStatisticsAsync(List<Statistic> statistics)
        {
            if (statistics == null || statistics.Count == 0)
                throw new ArgumentException("Список статистики порожній");
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                await _context.Statistics.AddRangeAsync(statistics);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
                
                return statistics;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}