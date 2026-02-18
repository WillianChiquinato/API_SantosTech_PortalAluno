using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly AppDbContext _efDbContext;

    public EnrollmentRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<Enrollment>> GetAllAsync()
    {
        return await _efDbContext.Enrollments.AsNoTracking().ToListAsync();
    }

    public async Task<Enrollment?> GetByIdAsync(int id)
    {
        return await _efDbContext.Enrollments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<Enrollment?> GetByUserIdAsync(int userId)
    {
        return _efDbContext.Enrollments.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
    }
}
