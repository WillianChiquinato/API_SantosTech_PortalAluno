using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class PhaseRepository : IPhaseRepository
{
    private readonly AppDbContext _efDbContext;

    public PhaseRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<Phase>> GetAllAsync()
    {
        return await _efDbContext.Phases.AsNoTracking().ToListAsync();
    }

    public async Task<Phase?> GetByIdAsync(int id)
    {
        return await _efDbContext.Phases.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }
}
