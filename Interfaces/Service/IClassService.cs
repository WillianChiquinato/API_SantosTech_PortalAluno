using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IClassService
{
    Task<CustomResponse<IEnumerable<Class>>> GetAllAsync();
    Task<CustomResponse<Class>> GetByIdAsync(int id);
    Task<CustomResponse<IEnumerable<IslandDTO>>> GetIslandsByUserIdAndCurrentModuleAsync(int userId, int moduleId);
    Task<CustomResponse<IEnumerable<ClassRoomDTO>>> GetClassRoomsByClassIdAsync(int classId);
    Task<CustomResponse<IEnumerable<Class>>> GetClassesByUserIdAsync(int userId);
}
