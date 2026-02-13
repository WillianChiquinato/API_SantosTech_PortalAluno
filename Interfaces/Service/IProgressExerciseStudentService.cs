using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IProgressExerciseStudentService
{
    Task<CustomResponse<IEnumerable<ProgressExerciseStudent>>> GetAllAsync();
    Task<CustomResponse<ProgressExerciseStudent>> GetByIdAsync(int id);
}
