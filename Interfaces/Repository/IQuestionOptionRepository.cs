using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IQuestionOptionRepository
{
    Task<List<QuestionOption>> GetAllAsync();
    Task<QuestionOption?> GetByIdAsync(int id);
}
