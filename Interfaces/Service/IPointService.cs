using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IPointService
{
    Task<CustomResponse<IEnumerable<Point>>> GetAllAsync();
    Task<CustomResponse<Point>> GetByIdAsync(int id);
    Task<CustomResponse<IEnumerable<PointRankingDTO>>> GetRankingAsync();
    Task<CustomResponse<IEnumerable<RankingCategoryDTO>>> GetRankingCategoriesAsync();
    Task<CustomResponse<IEnumerable<RankingPerCategoryDTO>>> GetAvailableRankingPerCategoryAsync(string? category, int? limit, int offset);
    Task<CustomResponse<IEnumerable<EventRankingDTO>>> GetRankingEventAsync(int eventType);
    Task<CustomResponse<IEnumerable<RankingEventHistoryDTO>>> GetRankingEventHistoryAsync(int? eventType, int limit, int offset);
    Task<CustomResponse<EventRankingDTO>> GetRankingEventByIdAsync(int id);
    Task<CustomResponse<RankingEventDTO>> CreateRankingEventAsync(RankingEventUpsertDTO request);
    Task<CustomResponse<RankingEventDTO>> UpdateRankingEventAsync(int id, RankingEventUpsertDTO request);
    Task<CustomResponse<bool>> DeleteRankingEventAsync(int id);
    Task<CustomResponse<ExercisePointAwardResult>> AddPointsForUserAsync(AddPointsDTO redeemPoints);
    Task<CustomResponse<bool>> ScheduleRankingEventAsync(int eventId);
}
