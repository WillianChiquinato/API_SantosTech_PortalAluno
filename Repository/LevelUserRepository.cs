using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class LevelUserRepository : ILevelUserRepository
{
    private readonly AppDbContext _efDbContext;

    public LevelUserRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<LevelUser?> GetByIdAsync(int id)
    {
        return await _efDbContext.LevelUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<LevelUser>> GetAllAsync()
    {
        return await _efDbContext.LevelUsers.AsNoTracking().ToListAsync();
    }
}