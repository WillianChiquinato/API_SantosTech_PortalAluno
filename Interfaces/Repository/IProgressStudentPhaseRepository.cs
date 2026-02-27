using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IProgressStudentPhaseRepository
{
    Task<List<ProgressStudentPhase>> GetAllAsync();
    Task<ProgressStudentPhase?> GetByIdAsync(int id);
    Task<ProgressStudentPhase?> GetProgressByUserIdAndPhaseIdAsync(int userId, int phaseId);
}
