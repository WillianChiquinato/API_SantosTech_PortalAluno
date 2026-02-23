using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IExerciseService
{
    Task<CustomResponse<IEnumerable<Exercise>>> GetAllAsync();
    Task<CustomResponse<Exercise>> GetByIdAsync(int id);
    Task<CustomResponse<IEnumerable<ExerciseDailyTasksDTO>>> GetDailyTasksForPhaseAsync(int phaseId, int userId);
    Task<CustomResponse<IEnumerable<QuestionOptionsDTO>>> GetQuestionsOptionsForExerciseAsync(int exerciseId);
    Task<CustomResponse<bool>> SubmitExerciseAnswersAsync(ExerciseSubmissionDTO submission);
}
