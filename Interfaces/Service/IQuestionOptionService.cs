using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IQuestionOptionService
{
    Task<CustomResponse<IEnumerable<QuestionOption>>> GetAllAsync();
    Task<CustomResponse<QuestionOption>> GetByIdAsync(int id);
}
