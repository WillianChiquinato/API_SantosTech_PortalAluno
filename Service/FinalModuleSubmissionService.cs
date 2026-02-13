using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class FinalModuleSubmissionService : IFinalModuleSubmissionService
{
    private readonly ILogger<FinalModuleSubmissionService> _logger;
    private readonly IFinalModuleSubmissionRepository _finalModuleSubmissionRepository;

    public FinalModuleSubmissionService(
        ILogger<FinalModuleSubmissionService> logger,
        IFinalModuleSubmissionRepository finalModuleSubmissionRepository)
    {
        _logger = logger;
        _finalModuleSubmissionRepository = finalModuleSubmissionRepository;
    }

    public async Task<CustomResponse<IEnumerable<FinalModuleSubmission>>> GetAllAsync()
    {
        var result = await _finalModuleSubmissionRepository.GetAllAsync();
        return CustomResponse<IEnumerable<FinalModuleSubmission>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<FinalModuleSubmission>> GetByIdAsync(int id)
    {
        var result = await _finalModuleSubmissionRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<FinalModuleSubmission>.Fail("Final module submission not found")
            : CustomResponse<FinalModuleSubmission>.SuccessTrade(result);
    }
}
