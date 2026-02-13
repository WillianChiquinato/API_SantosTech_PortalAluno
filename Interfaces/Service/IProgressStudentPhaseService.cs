using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IProgressStudentPhaseService
{
    Task<CustomResponse<IEnumerable<ProgressStudentPhase>>> GetAllAsync();
    Task<CustomResponse<ProgressStudentPhase>> GetByIdAsync(int id);
}
