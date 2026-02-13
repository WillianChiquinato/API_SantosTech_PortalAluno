using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class MaterialRepository : IMaterialRepository
{
    private readonly AppDbContext _efDbContext;

    public MaterialRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<Material>> GetAllAsync()
    {
        return await _efDbContext.Materials.AsNoTracking().ToListAsync();
    }

    public async Task<Material?> GetByIdAsync(int id)
    {
        return await _efDbContext.Materials.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }
}
