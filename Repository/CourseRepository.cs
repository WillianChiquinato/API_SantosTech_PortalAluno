using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
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

    public Task<List<Course>> GetCoursesAvailablesAsync()
    {
        return _efDbContext.Courses.AsNoTracking()
            .Where(c => c.IsPaid == false)
            .OrderBy(c => c.Id)
            .ToListAsync();
    }

    public Task<List<Course>> GetFullCoursesPaidAsync()
    {
        return _efDbContext.Courses.AsNoTracking()
            .Where(c => c.IsPaid)
            .ToListAsync();
    }

    public async Task<List<ClassCoursesDTO>> GetUserCoursesAsync(int userId)
    {
        var enrollMentsClassCourse = await _efDbContext.Enrollments
            .Where(e => e.UserId == userId)
            .Include(e => e.Class)
                .ThenInclude(c => c.Course)
            .Select(e => new ClassCoursesDTO
            {
                Id = e.Id,
                ClassName = e.Class.Name,
                ClassStartedAt = e.Class.StartDate,
                ClassFinishedAt = e.Class.EndDate,
                CourseName = e.Class.Course.Name,
                CourseDescription = e.Class.Course.Description,
                CourseDuration = e.Class.Course.Duration,
                CourseLevel = e.Class.Course.LevelDifficulty,
                CreatedAt = e.Class.Course.CreatedAt
            })
            .ToListAsync();

        return enrollMentsClassCourse;
    }
}
