using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IFinalModuleSubmissionRepository
{
    Task<List<FinalModuleSubmission>> GetAllAsync();
    Task<FinalModuleSubmission?> GetByIdAsync(int id);
}
