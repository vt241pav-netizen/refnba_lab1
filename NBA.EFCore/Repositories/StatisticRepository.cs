using Microsoft.EntityFrameworkCore;
using NBA.EFCore.Data;
using NBA.EFCore.EFModels;

namespace NBA.EFCore.Repositories
{
    public class StatisticRepository : IStatisticRepository
    {
        private readonly NbaDbContext _context;

        public StatisticRepository(NbaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Statistic>> GetAllAsync()
        {
            return await _context.Statistics
                .Where(s => !s.IsDeleted) 
                .Include(s => s.Player)
                .Include(s => s.Match)
                .OrderByDescending(s => s.Points)
                .ToListAsync();
        }

        public async Task<Statistic?> GetByIdAsync(int id)
        {
            return await _context.Statistics
                .Where(s => !s.IsDeleted && s.StatsId == id) 
                .Include(s => s.Player)
                .Include(s => s.Match)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Statistic statistic)
        {
            statistic.IsDeleted = false; 
            await _context.Statistics.AddAsync(statistic);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Statistic statistic)
        {
            var existingStat = await _context.Statistics
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.StatsId == statistic.StatsId);
            
            if (existingStat == null)
                throw new KeyNotFoundException($"Статистика з ID {statistic.StatsId} не знайдена");

            existingStat.MatchId = statistic.MatchId;
            existingStat.PlayerId = statistic.PlayerId;
            existingStat.Points = statistic.Points;
            existingStat.Rebounds = statistic.Rebounds;
            existingStat.Assists = statistic.Assists;
            existingStat.MinutesPlayed = statistic.MinutesPlayed;
            existingStat.Steals = statistic.Steals;
            existingStat.Blocks = statistic.Blocks;
            existingStat.Turnovers = statistic.Turnovers;
            existingStat.IsDeleted = statistic.IsDeleted;

            _context.Statistics.Update(existingStat);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var statistic = await _context.Statistics
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.StatsId == id);
            
            if (statistic != null)
            {
                statistic.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Statistic>> GetPlayerStatisticsAsync(int playerId)
        {
            return await _context.Statistics
                .Where(s => !s.IsDeleted && s.PlayerId == playerId)
                .Include(s => s.Match)
                .OrderByDescending(s => s.Match.GameDate)
                .ToListAsync();
        }

        public async Task<List<Statistic>> GetMatchStatisticsAsync(int matchId)
        {
            return await _context.Statistics
                .Where(s => !s.IsDeleted && s.MatchId == matchId)
                .Include(s => s.Player)
                    .ThenInclude(p => p.Team)
                .OrderByDescending(s => s.Points)
                .ToListAsync();
        }

        public async Task<List<Statistic>> GetDeletedStatisticsAsync()
        {
            return await _context.Statistics
                .IgnoreQueryFilters()
                .Where(s => s.IsDeleted)
                .Include(s => s.Player)
                .Include(s => s.Match)
                .OrderByDescending(s => s.Points)
                .ToListAsync();
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var statistic = await _context.Statistics
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.StatsId == id);
            
            if (statistic == null || !statistic.IsDeleted)
            {
                return false;
            }
            
            statistic.IsDeleted = false;
            await _context.SaveChangesAsync();
            
            return true;
        }
    }
}