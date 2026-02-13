using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IPhaseRepository
{
    Task<List<Phase>> GetAllAsync();
    Task<Phase?> GetByIdAsync(int id);
}
