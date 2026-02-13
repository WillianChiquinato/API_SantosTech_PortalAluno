using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class ClassService : IClassService
{
    private readonly ILogger<ClassService> _logger;
    private readonly IClassRepository _classRepository;

    public ClassService(ILogger<ClassService> logger, IClassRepository classRepository)
    {
        _logger = logger;
        _classRepository = classRepository;
    }

    public async Task<CustomResponse<IEnumerable<Class>>> GetAllAsync()
    {
        var result = await _classRepository.GetAllAsync();
        return CustomResponse<IEnumerable<Class>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<Class>> GetByIdAsync(int id)
    {
        var result = await _classRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<Class>.Fail("Class not found")
            : CustomResponse<Class>.SuccessTrade(result);
    }
}
