using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class PointService : IPointService
{
    private readonly ILogger<PointService> _logger;
    private readonly IPointRepository _pointRepository;

    public PointService(ILogger<PointService> logger, IPointRepository pointRepository)
    {
        _logger = logger;
        _pointRepository = pointRepository;
    }

    public async Task<CustomResponse<IEnumerable<Point>>> GetAllAsync()
    {
        var result = await _pointRepository.GetAllAsync();
        return CustomResponse<IEnumerable<Point>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<Point>> GetByIdAsync(int id)
    {
        var result = await _pointRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<Point>.Fail("Point not found")
            : CustomResponse<Point>.SuccessTrade(result);
    }
}
