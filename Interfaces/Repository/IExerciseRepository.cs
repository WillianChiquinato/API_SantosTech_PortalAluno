using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IExerciseRepository
{
    Task<List<Exercise>> GetAllAsync();
    Task<Exercise?> GetByIdAsync(int id);
    Task<List<Exercise?>> GetByIdsAsync(List<int> ids);
    Task<List<ExerciseDailyTasksDTO>> GetDailyTasksForPhaseAsync(int phaseId, int userId);
    Task<List<QuestionOptionsDTO>> GetQuestionsOptionsForExerciseAsync(int exerciseId);
    Task<bool> SubmitExerciseAnswersAsync(ExerciseSubmissionDTO submission);
    Task<List<ExerciseAnswerDTO>> GetDailyExercisesAnswersForPhaseAsync(int phaseId, int userId);
    Task<List<ExerciseDTO>> GetExercisesByPhaseId(int phaseId);
    Task<List<ExerciseAnswerDTO>> GetExercisesAnswersForUserAsync(int userId);
    Task<List<ExerciseAnswerDTO>> GetExercisesAnswersForUserByExerciseIdsAsync(int userId, List<int> exerciseIds);
    Task<List<UserExerciseFlow>> GetByUserAndPhaseOrderedAsync(int userId, int phaseId);
    Task CreateUserExerciseFlowAsync(List<UserExerciseFlow> userExerciseFlows);
    Task<List<Exercise>> GetFlowWithExercisesAsync(int userId, int phaseId);
    Task InsertLowerExercisesAsync(int userId, int phaseId, int exerciseId);
}
