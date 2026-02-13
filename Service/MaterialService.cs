using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class MaterialService : IMaterialService
{
    private readonly ILogger<MaterialService> _logger;
    private readonly IMaterialRepository _materialRepository;

    public MaterialService(ILogger<MaterialService> logger, IMaterialRepository materialRepository)
    {
        _logger = logger;
        _materialRepository = materialRepository;
    }

    public async Task<CustomResponse<IEnumerable<Material>>> GetAllAsync()
    {
        var result = await _materialRepository.GetAllAsync();
        return CustomResponse<IEnumerable<Material>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<Material>> GetByIdAsync(int id)
    {
        var result = await _materialRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<Material>.Fail("Material not found")
            : CustomResponse<Material>.SuccessTrade(result);
    }
}
