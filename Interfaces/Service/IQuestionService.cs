using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IQuestionService
{
    Task<CustomResponse<IEnumerable<Question>>> GetAllAsync();
    Task<CustomResponse<Question>> GetByIdAsync(int id);
}
