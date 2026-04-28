using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IPointService
{
    Task<CustomResponse<IEnumerable<Point>>> GetAllAsync();
    Task<CustomResponse<Point>> GetByIdAsync(int id);
    Task<CustomResponse<IEnumerable<PointRankingDTO>>> GetRankingAsync();
    Task<CustomResponse<IEnumerable<RankingPerCategoryDTO>>> GetAvailableRankingPerCategoryAsync();
    Task<CustomResponse<IEnumerable<EventRankingDTO>>> GetRankingEventAsync(int eventType);
    Task<CustomResponse<ExercisePointAwardResult>> AddPointsForUserAsync(AddPointsDTO redeemPoints);
    Task<CustomResponse<bool>> ScheduleRankingEventAsync(int eventId);
}
