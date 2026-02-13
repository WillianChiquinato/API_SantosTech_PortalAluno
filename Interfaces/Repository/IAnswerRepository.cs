using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IAnswerRepository
{
	Task<List<Answer>> GetAllAsync();
	Task<Answer?> GetByIdAsync(int id);
}