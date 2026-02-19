using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface ILogsRepository
{
    Task<List<Logs>> GetLogsAsync();
    Task<List<Logs?>> GetLogsByUserIdAsync(int userId);
    Task AddLogsAsync(Logs log);
}