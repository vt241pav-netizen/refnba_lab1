using Microsoft.EntityFrameworkCore;
using NBA.EFCore.Data;
using NBA.EFCore.EFModels;

namespace NBA.EFCore.Repositories
{
    public class MatchRepository : IMatchRepository
    {
        private readonly NbaDbContext _context;

        public MatchRepository(NbaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Match>> GetAllAsync()
        {
            return await _context.Matches
                .Where(m => !m.IsDeleted) 
                .Include(m => m.Statistics.Where(s => !s.IsDeleted))
                    .ThenInclude(s => s.Player)
                .OrderByDescending(m => m.GameDate)
                .ToListAsync();
        }

        public async Task<Match?> GetByIdAsync(int id)
        {
            return await _context.Matches
                .Where(m => !m.IsDeleted && m.MatchId == id) 
                .Include(m => m.Statistics.Where(s => !s.IsDeleted))
                    .ThenInclude(s => s.Player)
                        .ThenInclude(p => p.Team)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Match match)
        {
            match.IsDeleted = false; 
            await _context.Matches.AddAsync(match);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Match match)
        {
            var existingMatch = await _context.Matches
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.MatchId == match.MatchId);
            
            if (existingMatch == null)
                throw new KeyNotFoundException($"Матч з ID {match.MatchId} не знайдений");

            
            existingMatch.Season = match.Season;
            existingMatch.MatchType = match.MatchType;
            existingMatch.GameDate = match.GameDate;
            existingMatch.HomeTeamId = match.HomeTeamId;
            existingMatch.AwayTeamId = match.AwayTeamId;
            existingMatch.HomeTeamScore = match.HomeTeamScore;
            existingMatch.AwayTeamScore = match.AwayTeamScore;
            existingMatch.IsDeleted = match.IsDeleted;

            _context.Matches.Update(existingMatch);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var match = await _context.Matches
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.MatchId == id);
            
            if (match != null)
            {
                match.IsDeleted = true;
                
                var statistics = await _context.Statistics
                    .IgnoreQueryFilters()
                    .Where(s => s.MatchId == id)
                    .ToListAsync();
                
                foreach (var stat in statistics)
                {
                    stat.IsDeleted = true;
                }
                
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Match>> GetMatchesByTeamAsync(int teamId)
        {
            return await _context.Matches
                .Where(m => !m.IsDeleted && (m.HomeTeamId == teamId || m.AwayTeamId == teamId))
                .Include(m => m.Statistics.Where(s => !s.IsDeleted))
                .OrderByDescending(m => m.GameDate)
                .ToListAsync();
        }

        public async Task<List<Match>> GetMatchesByDateRangeAsync(DateTime date)
        {
            return await _context.Matches
                .Where(m => !m.IsDeleted && m.GameDate.Date == date.Date)
                .Include(m => m.Statistics.Where(s => !s.IsDeleted))
                .OrderBy(m => m.GameDate)
                .ToListAsync();
        }

        public async Task<List<Match>> GetMatchesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Matches
                .Where(m => !m.IsDeleted && m.GameDate.Date >= startDate.Date && m.GameDate.Date <= endDate.Date)
                .Include(m => m.Statistics.Where(s => !s.IsDeleted))
                .OrderBy(m => m.GameDate)
                .ToListAsync();
        }

        public async Task<List<Match>> GetDeletedMatchesAsync()
        {
            return await _context.Matches
                .IgnoreQueryFilters()
                .Where(m => m.IsDeleted)
                .Include(m => m.Statistics.Where(s => !s.IsDeleted))
                .OrderByDescending(m => m.GameDate)
                .ToListAsync();
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var match = await _context.Matches
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.MatchId == id);
            
            if (match == null || !match.IsDeleted)
            {
                return false;
            }
            
            match.IsDeleted = false;
            
            var statistics = await _context.Statistics
                .IgnoreQueryFilters()
                .Where(s => s.MatchId == id && s.IsDeleted)
                .ToListAsync();
            
            foreach (var stat in statistics)
            {
                stat.IsDeleted = false;
            }
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}