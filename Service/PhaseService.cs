using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class PhaseService : IPhaseService
{
    private readonly ILogger<PhaseService> _logger;
    private readonly IPhaseRepository _phaseRepository;

    public PhaseService(ILogger<PhaseService> logger, IPhaseRepository phaseRepository)
    {
        _logger = logger;
        _phaseRepository = phaseRepository;
    }

    public async Task<CustomResponse<IEnumerable<Phase>>> GetAllAsync()
    {
        var result = await _phaseRepository.GetAllAsync();
        return CustomResponse<IEnumerable<Phase>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<Phase>> GetByIdAsync(int id)
    {
        var result = await _phaseRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<Phase>.Fail("Phase not found")
            : CustomResponse<Phase>.SuccessTrade(result);
    }
}
