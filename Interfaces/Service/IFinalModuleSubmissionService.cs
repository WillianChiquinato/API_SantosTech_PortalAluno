using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IFinalModuleSubmissionService
{
    Task<CustomResponse<IEnumerable<FinalModuleSubmission>>> GetAllAsync();
    Task<CustomResponse<FinalModuleSubmission>> GetByIdAsync(int id);
}
