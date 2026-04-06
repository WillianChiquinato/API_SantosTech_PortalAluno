using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IPointRepository
{
    Task<List<Point>> GetAllAsync();
    Task<Point?> GetByIdAsync(int id);
    Task<List<PointRankingDTO>> GetRankingAsync();
    Task<ExercisePointAwardResult> AddPointsForUserAsync(int userId, int exerciseId);
}
