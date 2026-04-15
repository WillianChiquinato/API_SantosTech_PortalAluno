using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IBadgeService
{
    Task<CustomResponse<IEnumerable<Badge>>> GetAllAsync();
    Task<CustomResponse<Badge>> GetByIdAsync(int id);
    Task<CustomResponse<IEnumerable<GoalWithBadgesResponse>>> GetGoalsWithBadgesByCourseIdAsync(int courseId);
    Task<CustomResponse<bool>> UpdateActivatedGoalIdAsync(int goalRewardId, int userId);
    Task<CustomResponse<IEnumerable<ActivatedGoalResponse>>> GetActivatedGoalsByUserIdAsync(int userId);
    Task<CustomResponse<bool>> GoalRewardOperationAsync(int goalRewardId, int userId);
}
