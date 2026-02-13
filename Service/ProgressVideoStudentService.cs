using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class ProgressVideoStudentService : IProgressVideoStudentService
{
    private readonly ILogger<ProgressVideoStudentService> _logger;
    private readonly IProgressVideoStudentRepository _progressVideoStudentRepository;

    public ProgressVideoStudentService(
        ILogger<ProgressVideoStudentService> logger,
        IProgressVideoStudentRepository progressVideoStudentRepository)
    {
        _logger = logger;
        _progressVideoStudentRepository = progressVideoStudentRepository;
    }

    public async Task<CustomResponse<IEnumerable<ProgressVideoStudent>>> GetAllAsync()
    {
        var result = await _progressVideoStudentRepository.GetAllAsync();
        return CustomResponse<IEnumerable<ProgressVideoStudent>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<ProgressVideoStudent>> GetByIdAsync(int id)
    {
        var result = await _progressVideoStudentRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<ProgressVideoStudent>.Fail("Progress video student not found")
            : CustomResponse<ProgressVideoStudent>.SuccessTrade(result);
    }
}
