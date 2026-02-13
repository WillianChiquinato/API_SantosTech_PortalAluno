using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class BadgeRepository : IBadgeRepository
{
    private readonly AppDbContext _efDbContext;

    public BadgeRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<Badge>> GetAllAsync()
    {
        return await _efDbContext.Badges.AsNoTracking().ToListAsync();
    }

    public async Task<Badge?> GetByIdAsync(int id)
    {
        return await _efDbContext.Badges.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }
}
