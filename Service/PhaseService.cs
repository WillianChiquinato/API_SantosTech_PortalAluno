using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class PhaseService : IPhaseService
{
    private readonly ILogger<PhaseService> _logger;
    private readonly IPhaseRepository _phaseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IClassRepository _classRepository;

    public PhaseService(ILogger<PhaseService> logger, IPhaseRepository phaseRepository, IEnrollmentRepository enrollmentRepository, IClassRepository classRepository)
    {
        _logger = logger;
        _phaseRepository = phaseRepository;
        _enrollmentRepository = enrollmentRepository;
        _classRepository = classRepository;
    }

    public async Task<CustomResponse<IEnumerable<Phase>>> GetAllAsync()
    {
        try
        {
            var result = await _phaseRepository.GetAllAsync();
            return CustomResponse<IEnumerable<Phase>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all phases");
            return CustomResponse<IEnumerable<Phase>>.Fail("An error occurred while fetching the phases.");
        }
    }

    public async Task<CustomResponse<Phase>> GetByIdAsync(int id)
    {
        try
        {
            var result = await _phaseRepository.GetByIdAsync(id);
            return result == null
                ? CustomResponse<Phase>.Fail("Phase not found")
                : CustomResponse<Phase>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching phase with ID {PhaseId}", id);
            return CustomResponse<Phase>.Fail("An error occurred while fetching the phase.");
        }
    }
    
    public async Task<CustomResponse<CurrentModuleDTO>> GetCurrentModuleUserAsync(int userId, int enrollmentId)
    {
        try
        {
            var getEnrollMentUser = await _enrollmentRepository.GetByIdAsync(enrollmentId);
            var GetClassInUser = await _classRepository.GetByIdAsync(getEnrollMentUser?.ClassId ?? 0);

            var currentModuleClass = await _classRepository.GetCurrentModuleByClassIdAsync(GetClassInUser?.Id ?? 0);
            var totalPhasesPerModule = await _phaseRepository.GetTotalPhasesByModuleIdAsync(currentModuleClass?.Id ?? 0);
            
            var phaseUserDto = new CurrentModuleDTO
            {
                Id = GetClassInUser?.CurrentModuleId ?? 0,
                Name = currentModuleClass?.Name ?? "No current module",
                Description = currentModuleClass?.Description ?? "No description available",
                TotalPhases = totalPhasesPerModule
            };
            return CustomResponse<CurrentModuleDTO>.SuccessTrade(phaseUserDto, totalRows: totalPhasesPerModule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching current module for user {UserId}", userId);
            return CustomResponse<CurrentModuleDTO>.Fail("An error occurred while fetching the current module for the user.");
        }
    }
}
