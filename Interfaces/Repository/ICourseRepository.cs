using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface ICourseRepository
{
    Task<List<Course>> GetAllAsync();
    Task<Course?> GetByIdAsync(int id);
    Task<List<Course>> GetCoursesAvailablesAsync();
    Task<List<Course>> GetFullCoursesPaidAsync();
    Task<List<ClassCoursesDTO>> GetUserCoursesAsync(int userId);
}
