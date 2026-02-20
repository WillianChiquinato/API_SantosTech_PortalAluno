using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface ICourseService
{
    Task<CustomResponse<IEnumerable<Course>>> GetAllAsync();
    Task<CustomResponse<Course>> GetByIdAsync(int id);
    Task<CustomResponse<IEnumerable<Course>>> GetFullCoursesPaidAsync();
    Task<CustomResponse<IEnumerable<ProgressPaidCourses>>> GetProgressUserPaidCoursesAsync(int userId);
}
