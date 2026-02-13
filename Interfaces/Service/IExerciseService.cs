using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IExerciseService
{
    Task<CustomResponse<IEnumerable<Exercise>>> GetAllAsync();
    Task<CustomResponse<Exercise>> GetByIdAsync(int id);
}
