using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class TeamsChallengerRepository : ITeamsChallengerRepository
{
    private readonly AppDbContext _efDbContext;

    public TeamsChallengerRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<TeamsChallenger>> GetAllAsync()
    {
        return await _efDbContext.TeamsChallengers.AsNoTracking().ToListAsync();
    }

    public async Task<TeamsChallenger?> GetByIdAsync(int id)
    {
        return await _efDbContext.TeamsChallengers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }
}
