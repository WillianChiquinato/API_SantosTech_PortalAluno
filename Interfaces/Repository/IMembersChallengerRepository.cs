using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IMembersChallengerRepository
{
    Task<List<MembersChallenger>> GetAllAsync();
    Task<MembersChallenger?> GetByIdAsync(int id);
}
