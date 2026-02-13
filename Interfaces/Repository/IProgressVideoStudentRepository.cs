using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IProgressVideoStudentRepository
{
    Task<List<ProgressVideoStudent>> GetAllAsync();
    Task<ProgressVideoStudent?> GetByIdAsync(int id);
}
