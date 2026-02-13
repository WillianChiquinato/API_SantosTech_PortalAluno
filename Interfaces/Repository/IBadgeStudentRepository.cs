using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IBadgeStudentRepository
{
    Task<List<BadgeStudent>> GetAllAsync();
    Task<BadgeStudent?> GetByIdAsync(int id);
}
