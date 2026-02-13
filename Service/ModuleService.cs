using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class ModuleService : IModuleService
{
    private readonly ILogger<ModuleService> _logger;
    private readonly IModuleRepository _moduleRepository;

    public ModuleService(ILogger<ModuleService> logger, IModuleRepository moduleRepository)
    {
        _logger = logger;
        _moduleRepository = moduleRepository;
    }

    public async Task<CustomResponse<IEnumerable<Module>>> GetAllAsync()
    {
        var result = await _moduleRepository.GetAllAsync();
        return CustomResponse<IEnumerable<Module>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<Module>> GetByIdAsync(int id)
    {
        var result = await _moduleRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<Module>.Fail("Module not found")
            : CustomResponse<Module>.SuccessTrade(result);
    }
}
