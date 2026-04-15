using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IBadgeRepository
{
    Task<List<Badge>> GetAllAsync();
    Task<Badge?> GetByIdAsync(int id);
    Task<List<Badge?>> GetByUserIdAsync(int userId);
    Task<List<GoalWithBadgesResponse>> GetGoalsWithBadgesByCourseIdAsync(int courseId);
    Task<bool> UpdateActivatedGoalIdAsync(int goalRewardId, int userId);
    Task<List<ActivatedGoalResponse>> GetActivatedGoalsByUserIdAsync(int userId);
}
