using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IAnswerRepository
{
	Task<List<Answer>> GetAllAsync();
	Task<Answer?> GetByIdAsync(int id);
	Task<List<Answer>> GetByUserIdAsync(int userId);
	Task<int> GetNewAnswersByUserIdCount(int userId);
	Task<Dictionary<int, Exercise>> GetExercisesByAnswerIdsAsync(List<int> answerIds);
	Task<Dictionary<int, string>> GetSelectedOptionTextsByAnswerIdsAsync(List<int> answerIds);
}