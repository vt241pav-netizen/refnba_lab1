using NBA.EFCore.EFModels;

public interface IPlayerRepository
{
    Task<List<Player>> GetAllAsync(CancellationToken ct = default);
    Task<Player?> GetByIdAsync(int id, CancellationToken ct = default);
    Task CreateAsync(Player player, CancellationToken ct = default);
    Task UpdateAsync(Player player, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    
    Task<Player?> GetByIdOptimizedAsync(int id);
    
    Task<List<Player>> GetPlayersByTeamAsync(int teamId);
    Task<List<Player>> SearchPlayersAsync(string searchTerm);
    Task<List<Player>> GetActivePlayersAsync();
    
    Task<List<Player>> GetTopScorersAsync(int topN);
    
    Task<List<Player>> GetPlayersPagedAsync(int page, int pageSize, string? teamFilter = null, string? positionFilter = null, string? searchTerm = null);
    Task<List<Statistic>> GetPlayerStatisticsAsync(int playerId);
    
    Task<List<Player>> GetDeletedPlayersAsync();
    Task<bool> RestoreAsync(int id);
    Task<bool> HardDeleteAsync(int id);
    
}