using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface ITeamsChallengerService
{
    Task<CustomResponse<IEnumerable<TeamsChallenger>>> GetAllAsync();
    Task<CustomResponse<TeamsChallenger>> GetByIdAsync(int id);
}
