using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class ProgressVideoStudentRepository : IProgressVideoStudentRepository
{
    private readonly AppDbContext _efDbContext;

    public ProgressVideoStudentRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<ProgressVideoStudent>> GetAllAsync()
    {
        return await _efDbContext.ProgressVideoStudents.AsNoTracking().ToListAsync();
    }

    public async Task<ProgressVideoStudent?> GetByIdAsync(int id)
    {
        return await _efDbContext.ProgressVideoStudents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }
}
