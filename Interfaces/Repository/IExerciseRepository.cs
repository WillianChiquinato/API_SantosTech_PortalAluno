using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IExerciseRepository
{
    Task<List<Exercise>> GetAllAsync();
    Task<Exercise?> GetByIdAsync(int id);
    Task<List<ExerciseDailyTasksDTO>> GetDailyTasksForPhaseAsync(int phaseId, int userId);
    Task<List<QuestionOptionsDTO>> GetQuestionsOptionsForExerciseAsync(int exerciseId);
    Task<bool> SubmitExerciseAnswersAsync(ExerciseSubmissionDTO submission);
    Task<List<ExerciseAnswerDTO>> GetDailyExercisesAnswersForPhaseAsync(int phaseId, int userId);
    Task<List<ExerciseDTO>> GetExercisesByPhaseId(int phaseId);
    Task<List<ExerciseAnswerDTO>> GetExercisesAnswersForUserAsync(int userId);
}
