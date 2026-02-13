using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IAnswerService
{
	Task<CustomResponse<IEnumerable<Answer>>> GetAllAsync();
	Task<CustomResponse<Answer>> GetByIdAsync(int id);
}