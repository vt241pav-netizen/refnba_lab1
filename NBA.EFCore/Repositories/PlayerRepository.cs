using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NBA.EFCore.Data;
using NBA.EFCore.EFModels;

namespace NBA.EFCore.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly NbaDbContext _context;

        private static readonly Func<NbaDbContext, int, Task<Player?>> _getByIdCompiled =
            EF.CompileAsyncQuery((NbaDbContext ctx, int id) =>
                ctx.Players
                    .AsNoTracking() 
                    .Include(p => p.Team) 
                    .ThenInclude(t => t.Arena) 
                    .Include(p => p.Statistics)
                    .FirstOrDefault(p => p.PlayerId == id));

        public PlayerRepository(NbaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Player>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Players
                .Where(p => !p.IsDeleted) 
                .Include(p => p.Team)
                .ThenInclude(t => t.Arena)
                .Include(p => p.Statistics.Where(s => !s.IsDeleted))
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync(ct);
        }

        public async Task<Player?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Players
                .Where(p => !p.IsDeleted && p.PlayerId == id)
                .Include(p => p.Team)
                .Include(p => p.Statistics.Where(s => !s.IsDeleted))
                .FirstOrDefaultAsync(ct);
        }

        public async Task CreateAsync(Player player, CancellationToken ct = default)
        {
            var existingPlayer = await _context.Players
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.PlayerId == player.PlayerId, ct);
            
            if (existingPlayer != null)
                throw new InvalidOperationException($"Гравець з ID {player.PlayerId} вже існує");

            player.IsDeleted = false;
            
            if (!string.IsNullOrEmpty(player.Position))
                player.Position = player.Position.Trim();

            await _context.Players.AddAsync(player, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Player player, CancellationToken ct = default)
        {
            var existingPlayer = await _context.Players
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.PlayerId == player.PlayerId, ct);
            
            if (existingPlayer == null)
                throw new KeyNotFoundException($"Гравець з ID {player.PlayerId} не знайдений");

            existingPlayer.FirstName = player.FirstName;
            existingPlayer.LastName = player.LastName;
            existingPlayer.TeamId = player.TeamId;
            existingPlayer.Position = player.Position?.Trim() ?? existingPlayer.Position;
            existingPlayer.JerseyNumber = player.JerseyNumber;
            existingPlayer.BirthDate = player.BirthDate;
            existingPlayer.Country = player.Country;
            existingPlayer.HeightCm = player.HeightCm;
            existingPlayer.WeightKg = player.WeightKg;
            existingPlayer.DraftYear = player.DraftYear;
            existingPlayer.DraftRound = player.DraftRound;
            existingPlayer.DraftPick = player.DraftPick;
            existingPlayer.IsDeleted = player.IsDeleted;

            _context.Players.Update(existingPlayer);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var player = await _context.Players
                .IgnoreQueryFilters() 
                .FirstOrDefaultAsync(p => p.PlayerId == id, ct);
            
            if (player != null)
            {
                player.IsDeleted = true;
                
                var playerStats = await _context.Statistics
                    .IgnoreQueryFilters()
                    .Where(s => s.PlayerId == id)
                    .ToListAsync(ct);
                
                foreach (var stat in playerStats)
                {
                    stat.IsDeleted = true;
                }
                
                await _context.SaveChangesAsync(ct);
            }
        }
        public async Task<bool> HardDeleteAsync(int id)
        {
            var player = await _context.Players
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.PlayerId == id);
            
            if (player == null)
            {
                return false;
            }
            
            _context.Players.Remove(player);
            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task<Player?> GetByIdOptimizedAsync(int id)
        {
            return await _getByIdCompiled(_context, id);
        }

        public async Task<List<Player>> GetPlayersByTeamAsync(int teamId)
        {
            return await _context.Players
                .Where(p => p.TeamId == teamId && !p.IsDeleted)
                .Include(p => p.Team)
                .ThenInclude(t => t.Arena)
                .Include(p => p.Statistics.Where(s => !s.IsDeleted))
                .OrderBy(p => p.JerseyNumber)
                .ToListAsync();
        }

        public async Task<List<Player>> SearchPlayersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var searchTermLower = searchTerm.ToLower();
            
            return await _context.Players
                .Where(p => !p.IsDeleted && 
                       (p.FirstName.ToLower().Contains(searchTermLower) || 
                        p.LastName.ToLower().Contains(searchTermLower) ||
                        p.Team.TeamName.ToLower().Contains(searchTermLower) ||
                        p.Position.ToLower().Contains(searchTermLower) ||
                        p.Country.ToLower().Contains(searchTermLower)))
                .Include(p => p.Team)
                .ThenInclude(t => t.Arena)
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .Take(50) 
                .ToListAsync();
        }

        public async Task<List<Player>> GetActivePlayersAsync()
        {
            return await _context.Players
                .Where(p => !p.IsDeleted)
                .Include(p => p.Team)
                .ThenInclude(t => t.Arena)
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();
        }

        public async Task<List<Statistic>> GetPlayerStatisticsAsync(int playerId)
        {
            return await _context.Statistics
                .Where(s => s.PlayerId == playerId && !s.IsDeleted)
                .Include(s => s.Match)
                .ThenInclude(m => m.Statistics)
                .OrderByDescending(s => s.Match.GameDate)
                .ToListAsync();
        }

        public async Task<List<Player>> GetTopScorersAsync(int topN = 10)
        {
            return await _context.Players
                .Where(p => !p.IsDeleted)
                .Select(p => new
                {
                    Player = p,
                    TotalPoints = p.Statistics.Where(s => !s.IsDeleted).Sum(s => s.Points ?? 0)
                })
                .OrderByDescending(x => x.TotalPoints)
                .Take(topN)
                .Select(x => x.Player)
                .Include(p => p.Team)
                .ToListAsync();
        }

        public async Task<List<Player>> GetPlayersPagedAsync(int page, int pageSize, string? teamFilter = null, string? positionFilter = null, string? searchTerm = null)
        {
            var query = _context.Players
                .Where(p => !p.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(teamFilter))
            {
                query = query.Where(p => p.Team.TeamName.Contains(teamFilter));
            }

            if (!string.IsNullOrEmpty(positionFilter))
            {
                query = query.Where(p => p.Position.Contains(positionFilter));
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchTermLower = searchTerm.ToLower();
                query = query.Where(p => 
                    p.FirstName.ToLower().Contains(searchTermLower) || 
                    p.LastName.ToLower().Contains(searchTermLower));
            }

            return await query
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Player>> GetDeletedPlayersAsync()
        {
            return await _context.Players
                .IgnoreQueryFilters()
                .Where(p => p.IsDeleted)
                .Include(p => p.Team)
                .ThenInclude(t => t.Arena)
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var player = await _context.Players
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.PlayerId == id);
            
            if (player == null || !player.IsDeleted)
            {
                return false; 
            }
            
            player.IsDeleted = false;
            
            var playerStats = await _context.Statistics
                .IgnoreQueryFilters()
                .Where(s => s.PlayerId == id && s.IsDeleted)
                .ToListAsync();
            
            foreach (var stat in playerStats)
            {
                stat.IsDeleted = false;
            }
            
            await _context.SaveChangesAsync();
            return true;
        }

    
    }
}