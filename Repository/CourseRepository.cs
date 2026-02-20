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

    public Task<List<Course>> GetFullCoursesPaidAsync()
    {
        return _efDbContext.Courses.AsNoTracking()
            .Where(c => c.IsPaid)
            .ToListAsync();
    }

    public Task<List<ProgressPaidCourses>> GetProgressUserPaidCoursesAsync(int userId)
    {
        return _efDbContext.ProgressPaidCourses.AsNoTracking()
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }
}
