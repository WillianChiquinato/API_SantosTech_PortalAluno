using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IPointRepository
{
    Task<List<Point>> GetAllAsync();
    Task<Point?> GetByIdAsync(int id);
    Task<List<PointRankingDTO>> GetRankingAsync();
    Task<List<RankingPerCategoryDTO>> GetAvailableRankingPerCategoryAsync();
    Task<List<EventRankingDTO>> GetRankingEventAsync(int eventType);
    Task<RankingEvent?> ScheduleRankingEventAsync(int eventId);
    Task<ExercisePointAwardResult> AddPointsForUserAsync(int userId, int exerciseId);
    Task<Point> AddPointsAsync(Point point);
}
