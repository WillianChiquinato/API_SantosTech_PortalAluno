using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IClassRepository
{
    Task<List<Class>> GetAllAsync();
    Task<Class?> GetByIdAsync(int id);
    Task<Module?> GetModuleByPhaseIdAsync(int phaseId);
    Task<Module?> GetCurrentModuleByClassIdAsync(int classId);
    Task<IEnumerable<IslandPhaseDTO>> GetPhasesByCurrentModuleAsync(int moduleId);
    Task<Class?> GetByCourseAndModuleIdAsync(int courseId, int moduleId);
    Task<IEnumerable<ClassRoom>> GetClassRoomsByClassIdAsync(int classId);
    Task<IEnumerable<Class>> GetClassesByUserIdAsync(int userId);
}
