using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IPointRepository
{
    Task<List<Point>> GetAllAsync();
    Task<Point?> GetByIdAsync(int id);
    Task<List<Point>> GetRankingAsync();
    Task<bool> AddPointsForUserAsync(int userId, int pointsToAdd);
}
