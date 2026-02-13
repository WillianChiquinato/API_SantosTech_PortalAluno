using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class ProgressStudentPhaseService : IProgressStudentPhaseService
{
    private readonly ILogger<ProgressStudentPhaseService> _logger;
    private readonly IProgressStudentPhaseRepository _progressStudentPhaseRepository;

    public ProgressStudentPhaseService(
        ILogger<ProgressStudentPhaseService> logger,
        IProgressStudentPhaseRepository progressStudentPhaseRepository)
    {
        _logger = logger;
        _progressStudentPhaseRepository = progressStudentPhaseRepository;
    }

    public async Task<CustomResponse<IEnumerable<ProgressStudentPhase>>> GetAllAsync()
    {
        var result = await _progressStudentPhaseRepository.GetAllAsync();
        return CustomResponse<IEnumerable<ProgressStudentPhase>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<ProgressStudentPhase>> GetByIdAsync(int id)
    {
        var result = await _progressStudentPhaseRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<ProgressStudentPhase>.Fail("Progress student phase not found")
            : CustomResponse<ProgressStudentPhase>.SuccessTrade(result);
    }
}
