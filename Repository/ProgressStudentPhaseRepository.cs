using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class ProgressStudentPhaseRepository : IProgressStudentPhaseRepository
{
    private readonly AppDbContext _efDbContext;

    public ProgressStudentPhaseRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<ProgressStudentPhase>> GetAllAsync()
    {
        return await _efDbContext.ProgressStudentPhases.AsNoTracking().ToListAsync();
    }

    public async Task<ProgressStudentPhase?> GetByIdAsync(int id)
    {
        return await _efDbContext.ProgressStudentPhases.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ProgressStudentPhase?> GetProgressByUserIdAndPhaseIdAsync(int userId, int phaseId)
    {
        return await _efDbContext.ProgressStudentPhases.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.PhaseId == phaseId);
    }
}
