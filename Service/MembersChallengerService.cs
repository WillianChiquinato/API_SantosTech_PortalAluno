using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class MembersChallengerService : IMembersChallengerService
{
    private readonly ILogger<MembersChallengerService> _logger;
    private readonly IMembersChallengerRepository _membersChallengerRepository;

    public MembersChallengerService(
        ILogger<MembersChallengerService> logger,
        IMembersChallengerRepository membersChallengerRepository)
    {
        _logger = logger;
        _membersChallengerRepository = membersChallengerRepository;
    }

    public async Task<CustomResponse<IEnumerable<MembersChallenger>>> GetAllAsync()
    {
        var result = await _membersChallengerRepository.GetAllAsync();
        return CustomResponse<IEnumerable<MembersChallenger>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<MembersChallenger>> GetByIdAsync(int id)
    {
        var result = await _membersChallengerRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<MembersChallenger>.Fail("Members challenger not found")
            : CustomResponse<MembersChallenger>.SuccessTrade(result);
    }
}
