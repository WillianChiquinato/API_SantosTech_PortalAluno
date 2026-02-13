using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface ITeamsChallengerRepository
{
    Task<List<TeamsChallenger>> GetAllAsync();
    Task<TeamsChallenger?> GetByIdAsync(int id);
}
