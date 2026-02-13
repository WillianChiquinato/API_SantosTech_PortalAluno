using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IEnrollmentService
{
    Task<CustomResponse<IEnumerable<Enrollment>>> GetAllAsync();
    Task<CustomResponse<Enrollment>> GetByIdAsync(int id);
}
