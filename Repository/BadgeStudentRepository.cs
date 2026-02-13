using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class BadgeStudentRepository : IBadgeStudentRepository
{
    private readonly AppDbContext _efDbContext;

    public BadgeStudentRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<BadgeStudent>> GetAllAsync()
    {
        return await _efDbContext.BadgeStudents.AsNoTracking().ToListAsync();
    }

    public async Task<BadgeStudent?> GetByIdAsync(int id)
    {
        return await _efDbContext.BadgeStudents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }
}
