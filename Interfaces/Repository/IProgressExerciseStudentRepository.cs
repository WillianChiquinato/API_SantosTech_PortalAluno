using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IProgressExerciseStudentRepository
{
    Task<List<ProgressExerciseStudent>> GetAllAsync();
    Task<ProgressExerciseStudent?> GetByIdAsync(int id);
}
