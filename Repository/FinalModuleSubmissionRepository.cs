using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class FinalModuleSubmissionRepository : IFinalModuleSubmissionRepository
{
    private readonly AppDbContext _efDbContext;

    public FinalModuleSubmissionRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<FinalModuleSubmission>> GetAllAsync()
    {
        return await _efDbContext.FinalModuleSubmissions.AsNoTracking().ToListAsync();
    }

    public async Task<FinalModuleSubmission?> GetByIdAsync(int id)
    {
        return await _efDbContext.FinalModuleSubmissions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }
}
