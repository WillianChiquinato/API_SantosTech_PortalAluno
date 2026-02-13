using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class CourseService : ICourseService
{
    private readonly ILogger<CourseService> _logger;
    private readonly ICourseRepository _courseRepository;

    public CourseService(ILogger<CourseService> logger, ICourseRepository courseRepository)
    {
        _logger = logger;
        _courseRepository = courseRepository;
    }

    public async Task<CustomResponse<IEnumerable<Course>>> GetAllAsync()
    {
        var result = await _courseRepository.GetAllAsync();
        return CustomResponse<IEnumerable<Course>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<Course>> GetByIdAsync(int id)
    {
        var result = await _courseRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<Course>.Fail("Course not found")
            : CustomResponse<Course>.SuccessTrade(result);
    }
}
