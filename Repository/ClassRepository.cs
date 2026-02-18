using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class ClassRepository : IClassRepository
{
    private readonly AppDbContext _efDbContext;

    public ClassRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<Class>> GetAllAsync()
    {
        return await _efDbContext.Classes.AsNoTracking().ToListAsync();
    }

    public async Task<Class?> GetByIdAsync(int id)
    {
        return await _efDbContext.Classes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Module?> GetCurrentModuleByClassIdAsync(int classId)
    {
        var classEntity = await _efDbContext.Classes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == classId);
        if (classEntity == null)
        {
            return null;
        }

        return await _efDbContext.Modules.AsNoTracking().FirstOrDefaultAsync(x => x.Id == classEntity.CurrentModuleId);
    }
}
