using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class ModuleRepository : IModuleRepository
{
    private readonly AppDbContext _efDbContext;

    public ModuleRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<Module>> GetAllAsync()
    {
        return await _efDbContext.Modules.AsNoTracking().ToListAsync();
    }

    public async Task<Module?> GetByIdAsync(int id)
    {
        return await _efDbContext.Modules.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }
}
