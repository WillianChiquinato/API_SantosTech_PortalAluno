using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IMembersChallengerService
{
    Task<CustomResponse<IEnumerable<MembersChallenger>>> GetAllAsync();
    Task<CustomResponse<MembersChallenger>> GetByIdAsync(int id);
}
