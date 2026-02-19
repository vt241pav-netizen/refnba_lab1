using Microsoft.EntityFrameworkCore;
using NBA.EFCore.Data;
using NBA.EFCore.EFModels;

namespace NBA.EFCore.Repositories
{
    public class CoachRepository : ICoachRepository
    {
        private readonly NbaDbContext _context;

        public CoachRepository(NbaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Coach>> GetAllAsync()
        {
            return await _context.Coaches
                .Where(c => !c.IsDeleted) 
                .Include(c => c.Team)
                .ThenInclude(t => t.Arena)
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }

        public async Task<Coach?> GetByIdAsync(int id)
        {
            return await _context.Coaches
                .Where(c => !c.IsDeleted && c.CoachId == id) 
                .Include(c => c.Team)
                .ThenInclude(t => t.Arena)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Coach coach)
        {
            coach.IsDeleted = false; 
            await _context.Coaches.AddAsync(coach);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Coach coach)
        {
            var existingCoach = await _context.Coaches
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.CoachId == coach.CoachId);
            
            if (existingCoach == null)
                throw new KeyNotFoundException($"Тренер з ID {coach.CoachId} не знайдений");

            existingCoach.FirstName = coach.FirstName;
            existingCoach.LastName = coach.LastName;
            existingCoach.TeamId = coach.TeamId;
            existingCoach.Role = coach.Role;
            existingCoach.StartDate = coach.StartDate;
            existingCoach.EndDate = coach.EndDate;
            existingCoach.IsDeleted = coach.IsDeleted;

            _context.Coaches.Update(existingCoach);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var coach = await _context.Coaches
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.CoachId == id);
            
            if (coach != null)
            {
                coach.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Coach>> GetDeletedCoachesAsync()
        {
            return await _context.Coaches
                .IgnoreQueryFilters()
                .Where(c => c.IsDeleted)
                .Include(c => c.Team)
                .ThenInclude(t => t.Arena)
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var coach = await _context.Coaches
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.CoachId == id);
            
            if (coach == null || !coach.IsDeleted)
            {
                return false; 
            }
            
            coach.IsDeleted = false;
            await _context.SaveChangesAsync();
            
            return true;
        }

        
    }
}