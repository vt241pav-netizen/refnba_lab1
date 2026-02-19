using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NBA.EFCore.Data;
using NBA.EFCore.DTOs;
using NBA.EFCore.EFModels;

namespace NBA.EFCore.Services
{
    public class NbaService
    {
        private readonly NbaDbContext _context;

        public NbaService(NbaDbContext context)
        {
            _context = context;
        }


        public async Task<List<Team>> GetTeamsWithRostersAsync()
{
    return await _context.Teams
        .Include(t => t.Players.Where(p => !p.IsDeleted)) 
        .ThenInclude(p => p.Statistics)
        .Include(t => t.Arena)
        .Include(t => t.Coaches)
        .AsSplitQuery()
        .ToListAsync();
}

        public async Task<List<PlayerDto>> GetPlayerProjectionsAsync()
        {
            return await _context.Players
                .Select(p => new PlayerDto
                {
                    FullName = p.FirstName + " " + p.LastName,
                    TeamName = p.Team.TeamName,
                    Position = p.Position,
                    Age = DateTime.Now.Year - p.BirthDate.Year
                })
                .ToListAsync();
        }

      public async Task<List<TeamStatsDto>> GetTeamStatsAsync()
{
    var teams = await _context.Teams
        .Include(t => t.Players.Where(p => !p.IsDeleted))
        .ToListAsync();

    var allMatches = await _context.Matches.ToListAsync();

    var result = new List<TeamStatsDto>();

    foreach (var team in teams)
    {
        var teamMatches = allMatches
            .Where(m => m.HomeTeamId == team.TeamId || m.AwayTeamId == team.TeamId)
            .ToList();
        
        int wins = 0;
        int totalPointsScored = 0;
        int totalPointsConceded = 0;
        
        foreach (var match in teamMatches)
        {
            bool isHome = match.HomeTeamId == team.TeamId;
            int teamScore = isHome ? match.HomeTeamScore : match.AwayTeamScore;
            int opponentScore = isHome ? match.AwayTeamScore : match.HomeTeamScore;
            
            totalPointsScored += teamScore;
            
            if (teamScore > opponentScore)
                wins++;
        }
        
        var dto = new TeamStatsDto
        {
            TeamName = team.TeamName,
            PlayerCount = team.Players.Count,
            AverageHeight = team.Players.Any() ? 
                (double)team.Players.Average(p => p.HeightCm) : 0,
            TotalMatches = teamMatches.Count,
            Wins = wins,
            Losses = teamMatches.Count - wins,
            AveragePointsScored = teamMatches.Any() ? 
                Math.Round((double)totalPointsScored / teamMatches.Count, 1) : 0,
            AveragePointsConceded = teamMatches.Any() ? 
                Math.Round((double)totalPointsConceded / teamMatches.Count, 1) : 0
        };
        
        result.Add(dto);
    }
    
    return result.OrderByDescending(t => t.Wins)
                 .ThenByDescending(t => t.AveragePointsScored)
                 .ToList();
}

        public async Task<List<Player>> GetPlayersPagedAsync(int page, int pageSize, string? teamFilter = null)
        {
            var query = _context.Players.AsQueryable();

            if (!string.IsNullOrEmpty(teamFilter))
            {
                query = query.Where(p => p.Team.TeamName.Contains(teamFilter));
            }

            return await query
                .OrderBy(p => p.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
public async Task<TeamDetailedStatsDto?> GetTeamDetailedStatsAsync(int teamId)
{
    var team = await _context.Teams
        .Include(t => t.Players.Where(p => !p.IsDeleted))
        .Include(t => t.Arena)
        .Include(t => t.Division)
        .FirstOrDefaultAsync(t => t.TeamId == teamId);
    
    if (team == null) return null;
    
    var homeMatches = await _context.Matches
        .Where(m => m.HomeTeamId == teamId)
        .ToListAsync();
    
    var awayMatches = await _context.Matches
        .Where(m => m.AwayTeamId == teamId)
        .ToListAsync();
    
    var allMatches = homeMatches.Concat(awayMatches).ToList();
    
    int wins = homeMatches.Count(m => m.HomeTeamScore > m.AwayTeamScore) +
               awayMatches.Count(m => m.AwayTeamScore > m.HomeTeamScore);
    
    int totalPointsScored = homeMatches.Sum(m => m.HomeTeamScore) + 
                           awayMatches.Sum(m => m.AwayTeamScore);
    int totalPointsConceded = homeMatches.Sum(m => m.AwayTeamScore) + 
                             awayMatches.Sum(m => m.HomeTeamScore);
    
    return new TeamDetailedStatsDto
    {
        TeamName = team.TeamName,
        ArenaName = team.Arena?.ArenaName ?? "Немає",
        DivisionName = team.Division?.DivisionName ?? "Немає",
        PlayerCount = team.Players.Count,
        AverageHeight = team.Players.Any() ? 
            (double)team.Players.Average(p => p.HeightCm) : 0,
        TotalMatches = allMatches.Count,
        Wins = wins,
        Losses = allMatches.Count - wins,
        TotalPointsScored = totalPointsScored,
        TotalPointsConceded = totalPointsConceded,
        AveragePointsScored = allMatches.Any() ? 
            Math.Round((double)totalPointsScored / allMatches.Count, 1) : 0,
        AveragePointsConceded = allMatches.Any() ? 
            Math.Round((double)totalPointsConceded / allMatches.Count, 1) : 0,
        WinPercentage = allMatches.Any() ? 
            Math.Round((double)wins / allMatches.Count * 100, 1) : 0
    };
}



        public async Task TradePlayerAsync(int playerId, int newTeamId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var player = await _context.Players.FindAsync(playerId);
                if (player == null) throw new Exception("Player not found");

                player.TeamId = newTeamId;
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                Console.WriteLine($"[Transaction] Player {playerId} moved to team {newTeamId} successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"[Transaction] Error: {ex.Message}. Rolled back.");
                throw;
            }
        }

        public async Task UpdateArenaCapacityAdoNetAsync(int arenaId, int newCapacity)
{
    
    var connectionString = _context.Database.GetConnectionString();
    
    using var efTransaction = await _context.Database.BeginTransactionAsync();
    
    try
    {
        var dbTransaction = efTransaction.GetDbTransaction();
        var connection = dbTransaction.Connection as SqlConnection;
        
        if (connection == null)
        {
            throw new InvalidOperationException("Не вдалося отримати SqlConnection з транзакції");
        }
        
        
        var adoCmd = connection.CreateCommand();
        adoCmd.Transaction = dbTransaction as SqlTransaction;
        adoCmd.CommandText = @"
            UPDATE ARENAS 
            SET CAPACITY = @Capacity 
            WHERE ARENA_ID = @ArenaId";
        
        adoCmd.Parameters.Add(new SqlParameter("@Capacity", newCapacity));
        adoCmd.Parameters.Add(new SqlParameter("@ArenaId", arenaId));
        
        int rowsAffected = await adoCmd.ExecuteNonQueryAsync();
        
        if (rowsAffected == 0)
        {
            await efTransaction.RollbackAsync();
        }
        
        var arena = await _context.Arenas.FindAsync(arenaId);
        
        if (arena == null)
        {
            await efTransaction.RollbackAsync();
            throw new Exception($"Арена з ID {arenaId} не знайдена в EF операції");
        }
        
        await _context.SaveChangesAsync();
        
        var logCmd = connection.CreateCommand();
        logCmd.Transaction = dbTransaction as SqlTransaction;
        logCmd.CommandText = @"
            INSERT INTO ArenaLog (ArenaId, Action, ActionDate) 
            VALUES (@ArenaId, @Action, @ActionDate)";
        
        logCmd.Parameters.Add(new SqlParameter("@ArenaId", arenaId));
        logCmd.Parameters.Add(new SqlParameter("@Action", $"Capacity updated from {arena.Capacity} to {newCapacity}"));
        logCmd.Parameters.Add(new SqlParameter("@ActionDate", DateTime.Now));
        
        await logCmd.ExecuteNonQueryAsync();
        
        await efTransaction.CommitAsync();
        Console.WriteLine($"[Mixed Transaction]  Транзакція успішно завершена!");
        Console.WriteLine($"[Mixed Transaction] Стара місткість: {arena.Capacity}");
        Console.WriteLine($"[Mixed Transaction] Нова місткість: {newCapacity}");
    }
    catch (SqlException sqlEx)
    {
        await efTransaction.RollbackAsync();
        Console.WriteLine($"[Mixed Transaction]  SQL помилка: {sqlEx.Message}");
        throw new Exception($"Помилка бази даних: {sqlEx.Message}", sqlEx);
    }
    catch (Exception ex)
    {
        await efTransaction.RollbackAsync();
        Console.WriteLine($"[Mixed Transaction]  Загальна помилка: {ex.Message}");
        throw new Exception($"Помилка транзакції: {ex.Message}", ex);
    }
}
    }
    
    public class AuthorizationService
    {
        private readonly User? _currentUser;

        public AuthorizationService(User? currentUser)
        {
            _currentUser = currentUser;
        }

        public bool CanView => _currentUser != null;
        
        public bool CanCreate => _currentUser?.UserRole == "Developer" || 
                                _currentUser?.UserRole == "Admin";
        
        public bool CanEdit => _currentUser?.UserRole == "Developer" || 
                              _currentUser?.UserRole == "Admin";
        
        public bool CanDelete => _currentUser?.UserRole == "Admin"; 
        
        public bool CanManageUsers => _currentUser?.UserRole == "Admin";
        
        public bool CanRunReports => _currentUser != null;
        
        public bool CanViewStatistics => _currentUser != null;

        public void CheckViewPermission()
        {
            if (!CanView)
                throw new UnauthorizedAccessException("Для перегляду потрібно увійти в систему");
        }
        
        public void CheckCreatePermission()
        {
            if (!CanCreate)
                throw new UnauthorizedAccessException("Недостатньо прав для створення");
        }
        
        public void CheckEditPermission()
        {
            if (!CanEdit)
                throw new UnauthorizedAccessException("Недостатньо прав для редагування");
        }
        
        public void CheckDeletePermission()
        {
            if (!CanDelete)
                throw new UnauthorizedAccessException("Недостатньо прав для видалення");
        }
        
        public void CheckManageUsersPermission()
        {
            if (!CanManageUsers)
                throw new UnauthorizedAccessException("Управління користувачами тільки для адміністраторів");
        }
    }
}