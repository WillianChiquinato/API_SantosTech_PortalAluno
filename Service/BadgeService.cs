using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class BadgeService : IBadgeService
{
    private readonly ILogger<BadgeService> _logger;
    private readonly IBadgeRepository _badgeRepository;

    public BadgeService(ILogger<BadgeService> logger, IBadgeRepository badgeRepository)
    {
        _logger = logger;
        _badgeRepository = badgeRepository;
    }

    public async Task<CustomResponse<IEnumerable<Badge>>> GetAllAsync()
    {
        var result = await _badgeRepository.GetAllAsync();
        return CustomResponse<IEnumerable<Badge>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<Badge>> GetByIdAsync(int id)
    {
        var result = await _badgeRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<Badge>.Fail("Badge not found")
            : CustomResponse<Badge>.SuccessTrade(result);
    }
}
