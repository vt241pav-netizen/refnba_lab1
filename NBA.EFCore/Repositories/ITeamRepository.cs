using NBA.EFCore.EFModels;

namespace NBA.EFCore.Repositories
{
    public interface ITeamRepository
{
    Task<List<Team>> GetAllAsync();
    Task<Team?> GetByIdAsync(int id);
    Task CreateAsync(Team team);
    Task UpdateAsync(Team team);
    Task DeleteAsync(int id);
    Task<List<Team>> GetTeamsWithDetailsAsync();
    Task<List<Team>> GetDeletedTeamsAsync();
    Task<bool> RestoreAsync(int id); 
}
    public interface ICoachRepository
    {
        Task<List<Coach>> GetAllAsync();
    Task<Coach?> GetByIdAsync(int id);
    Task CreateAsync(Coach coach);
    Task UpdateAsync(Coach coach);
    Task DeleteAsync(int id); 
    Task<List<Coach>> GetDeletedCoachesAsync();
    Task<bool> RestoreAsync(int id);
    }

    public interface IMatchRepository
    {
         Task<List<Match>> GetAllAsync();
    Task<Match?> GetByIdAsync(int id);
    Task CreateAsync(Match match);
    Task UpdateAsync(Match match);
    Task DeleteAsync(int id);
    Task<List<Match>> GetMatchesByTeamAsync(int teamId);
    Task<List<Match>> GetMatchesByDateRangeAsync(DateTime date);
    Task<List<Match>> GetMatchesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<Match>> GetDeletedMatchesAsync(); 
    Task<bool> RestoreAsync(int id); 
    }

    public interface IStatisticRepository
    {
        Task<List<Statistic>> GetAllAsync();
    Task<Statistic?> GetByIdAsync(int id);
    Task CreateAsync(Statistic statistic);
    Task UpdateAsync(Statistic statistic);
    Task DeleteAsync(int id);
    Task<List<Statistic>> GetPlayerStatisticsAsync(int playerId);
    Task<List<Statistic>> GetMatchStatisticsAsync(int matchId);
    Task<List<Statistic>> GetDeletedStatisticsAsync(); 
    Task<bool> RestoreAsync(int id); 
    }
}