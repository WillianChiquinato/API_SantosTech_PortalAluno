using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class TeamsChallengerService : ITeamsChallengerService
{
    private readonly ILogger<TeamsChallengerService> _logger;
    private readonly ITeamsChallengerRepository _teamsChallengerRepository;

    public TeamsChallengerService(
        ILogger<TeamsChallengerService> logger,
        ITeamsChallengerRepository teamsChallengerRepository)
    {
        _logger = logger;
        _teamsChallengerRepository = teamsChallengerRepository;
    }

    public async Task<CustomResponse<IEnumerable<TeamsChallenger>>> GetAllAsync()
    {
        var result = await _teamsChallengerRepository.GetAllAsync();
        return CustomResponse<IEnumerable<TeamsChallenger>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<TeamsChallenger>> GetByIdAsync(int id)
    {
        var result = await _teamsChallengerRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<TeamsChallenger>.Fail("Teams challenger not found")
            : CustomResponse<TeamsChallenger>.SuccessTrade(result);
    }
}
