
using Microsoft.EntityFrameworkCore;
using NBA.EFCore.Data;
using NBA.EFCore.EFModels;

namespace NBA.EFCore.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly NbaDbContext _context;

        public TeamRepository(NbaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Team>> GetAllAsync()
    {
        return await _context.Teams
            .Where(t => !t.IsDeleted) 
            .Include(t => t.Arena)
            .Include(t => t.Players.Where(p => !p.IsDeleted))
            .Include(t => t.Coaches.Where(c => !c.IsDeleted))
            .ToListAsync();
    }
    
    public async Task<Team?> GetByIdAsync(int id)
    {
        return await _context.Teams
            .Where(t => !t.IsDeleted && t.TeamId == id)
            .Include(t => t.Arena)
            .Include(t => t.Players.Where(p => !p.IsDeleted))
            .Include(t => t.Coaches.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync();
    }


        public async Task CreateAsync(Team team)
        {
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();
        }

 public async Task DeleteAsync(int id)
    {
        var team = await _context.Teams
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TeamId == id);
        
        if (team != null)
        {
            team.IsDeleted = true;
            
            var players = await _context.Players
                .IgnoreQueryFilters()
                .Where(p => p.TeamId == id)
                .ToListAsync();
            
            foreach (var player in players)
            {
                player.IsDeleted = true;
            }
            
            var coaches = await _context.Coaches
                .IgnoreQueryFilters()
                .Where(c => c.TeamId == id)
                .ToListAsync();
            
            foreach (var coach in coaches)
            {
                coach.IsDeleted = true;
            }
            
            await _context.SaveChangesAsync();
        }
    }

        public async Task UpdateAsync(Team team)
        {
            _context.Teams.Update(team);
            await _context.SaveChangesAsync();
        }

         public async Task<List<Team>> GetDeletedTeamsAsync()
    {
        return await _context.Teams
            .IgnoreQueryFilters()
            .Where(t => t.IsDeleted)
            .Include(t => t.Arena)
            .Include(t => t.Division)
            .ToListAsync();
    }

    public async Task<bool> RestoreAsync(int id)
    {
        var team = await _context.Teams
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TeamId == id);
        
        if (team == null || !team.IsDeleted)
        {
            return false;
        }
        
        team.IsDeleted = false;
        
        var players = await _context.Players
            .IgnoreQueryFilters()
            .Where(p => p.TeamId == id && p.IsDeleted)
            .ToListAsync();
        
        foreach (var player in players)
        {
            player.IsDeleted = false;
        }
        
        var coaches = await _context.Coaches
            .IgnoreQueryFilters()
            .Where(c => c.TeamId == id && c.IsDeleted)
            .ToListAsync();
        
        foreach (var coach in coaches)
        {
            coach.IsDeleted = false;
        }
        
        await _context.SaveChangesAsync();
        return true;
    }

        public async Task<List<Team>> GetTeamsWithDetailsAsync()
        {
            return await _context.Teams
                .Include(t => t.Arena)
                .Include(t => t.Division)
                .ThenInclude(d => d.Conference)
                .Include(t => t.Players)
                .Include(t => t.Coaches)
                .AsSplitQuery()
                .ToListAsync();
        }

        
    }
}