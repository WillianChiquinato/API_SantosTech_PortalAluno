using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class ExerciseService : IExerciseService
{
    private readonly ILogger<ExerciseService> _logger;
    private readonly IExerciseRepository _exerciseRepository;

    public ExerciseService(ILogger<ExerciseService> logger, IExerciseRepository exerciseRepository)
    {
        _logger = logger;
        _exerciseRepository = exerciseRepository;
    }

    public async Task<CustomResponse<IEnumerable<Exercise>>> GetAllAsync()
    {
        var result = await _exerciseRepository.GetAllAsync();
        return CustomResponse<IEnumerable<Exercise>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<Exercise>> GetByIdAsync(int id)
    {
        var result = await _exerciseRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<Exercise>.Fail("Exercise not found")
            : CustomResponse<Exercise>.SuccessTrade(result);
    }
}
