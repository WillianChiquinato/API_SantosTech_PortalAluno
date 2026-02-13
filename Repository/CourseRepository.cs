using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _efDbContext;

    public CourseRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<Course>> GetAllAsync()
    {
        return await _efDbContext.Courses.AsNoTracking().ToListAsync();
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        return await _efDbContext.Courses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }
}
