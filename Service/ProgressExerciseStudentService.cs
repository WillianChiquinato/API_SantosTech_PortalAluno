using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class ProgressExerciseStudentService : IProgressExerciseStudentService
{
    private readonly ILogger<ProgressExerciseStudentService> _logger;
    private readonly IProgressExerciseStudentRepository _progressExerciseStudentRepository;

    public ProgressExerciseStudentService(
        ILogger<ProgressExerciseStudentService> logger,
        IProgressExerciseStudentRepository progressExerciseStudentRepository)
    {
        _logger = logger;
        _progressExerciseStudentRepository = progressExerciseStudentRepository;
    }

    public async Task<CustomResponse<IEnumerable<ProgressExerciseStudent>>> GetAllAsync()
    {
        var result = await _progressExerciseStudentRepository.GetAllAsync();
        return CustomResponse<IEnumerable<ProgressExerciseStudent>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<ProgressExerciseStudent>> GetByIdAsync(int id)
    {
        var result = await _progressExerciseStudentRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<ProgressExerciseStudent>.Fail("Progress exercise student not found")
            : CustomResponse<ProgressExerciseStudent>.SuccessTrade(result);
    }
}
