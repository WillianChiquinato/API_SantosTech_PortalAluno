using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IProgressVideoStudentService
{
    Task<CustomResponse<IEnumerable<ProgressVideoStudent>>> GetAllAsync();
    Task<CustomResponse<ProgressVideoStudent>> GetByIdAsync(int id);
}
