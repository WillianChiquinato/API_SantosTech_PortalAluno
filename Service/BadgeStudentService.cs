using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class BadgeStudentService : IBadgeStudentService
{
    private readonly ILogger<BadgeStudentService> _logger;
    private readonly IBadgeStudentRepository _badgeStudentRepository;

    public BadgeStudentService(ILogger<BadgeStudentService> logger, IBadgeStudentRepository badgeStudentRepository)
    {
        _logger = logger;
        _badgeStudentRepository = badgeStudentRepository;
    }

    public async Task<CustomResponse<IEnumerable<BadgeStudent>>> GetAllAsync()
    {
        var result = await _badgeStudentRepository.GetAllAsync();
        return CustomResponse<IEnumerable<BadgeStudent>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<BadgeStudent>> GetByIdAsync(int id)
    {
        var result = await _badgeStudentRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<BadgeStudent>.Fail("Badge student not found")
            : CustomResponse<BadgeStudent>.SuccessTrade(result);
    }
}
