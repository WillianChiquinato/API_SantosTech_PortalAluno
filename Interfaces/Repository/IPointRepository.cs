using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IPointRepository
{
    Task<List<Point>> GetAllAsync();
    Task<Point?> GetByIdAsync(int id);
    Task<List<PointRankingDTO>> GetRankingAsync();
    Task<List<RankingCategoryDTO>> GetRankingCategoriesAsync();
    Task<(List<RankingPerCategoryDTO> Items, int TotalRows)> GetAvailableRankingPerCategoryAsync(string? category, int? limit, int offset);
    Task<List<EventRankingDTO>> GetRankingEventAsync(int eventType);
    Task<(List<RankingEventHistoryDTO> Items, int TotalRows)> GetRankingEventHistoryAsync(int? eventType, int limit, int offset);
    Task<EventRankingDTO?> GetRankingEventByIdAsync(int id);
    Task<RankingEventDTO> CreateRankingEventAsync(RankingEventUpsertDTO request);
    Task<RankingEventDTO?> UpdateRankingEventAsync(int id, RankingEventUpsertDTO request);
    Task<RankingEvent?> DeleteRankingEventAsync(int id);
    Task<RankingEvent?> ScheduleRankingEventAsync(int eventId);
    Task UpdateRankingEventScheduledJobIdAsync(int eventId, string? scheduledJobId);
    Task<ExercisePointAwardResult> AddPointsForUserAsync(int userId, int exerciseId);
    Task<Point> AddPointsAsync(Point point);
}
