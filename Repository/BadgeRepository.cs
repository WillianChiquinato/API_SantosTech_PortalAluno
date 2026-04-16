using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class BadgeRepository : IBadgeRepository
{
    private readonly AppDbContext _efDbContext;
    private readonly IPointRepository _pointRepository;

    public BadgeRepository(AppDbContext efDbContext, IPointRepository pointRepository)
    {
        _efDbContext = efDbContext;
        _pointRepository = pointRepository;
    }

    public async Task<List<ActivatedGoalResponse>> GetActivatedGoalsByUserAndCourseIdAsync(int userId, int courseId)
    {
        var activatedGoals = await _efDbContext.GoalStudents
            .AsNoTracking()
            .Where(gs => gs.UserId == userId && gs.CourseId == courseId)
            .Include(gs => gs.GoalReward)
                .ThenInclude(gr => gr.Goal)
            .Select(gs => new ActivatedGoalResponse
            {
                Id = gs.Id,
                UserId = gs.UserId,
                GoalRewardId = gs.GoalRewardId,
                GoalName = gs.GoalReward!.Goal!.Name,
                GoalDescription = gs.GoalReward.Goal.Description,
                GoalType = gs.GoalReward.Goal.Type ?? GoalType.Custom,
                RewardType = gs.GoalReward.RewardType,
                CourseId = gs.CourseId,
                Progress = gs.Progress,
                IsCompleted = gs.IsCompleted,
                CompletedAt = gs.CompletedAt,
                RewardClaimed = gs.RewardClaimed,
                RewardClaimedAt = gs.RewardClaimedAt
            })
            .ToListAsync();

        var goalIds = activatedGoals.Select(ag => ag.GoalRewardId).ToList();

        var goalRewards = await _efDbContext.GoalRewards
            .AsNoTracking()
            .Where(gr => goalIds.Contains(gr.GoalId))
            .ToListAsync();

        var result = activatedGoals.Select(ag => new ActivatedGoalResponse
        {
            Id = ag.Id,
            UserId = ag.UserId,
            GoalRewardId = ag.GoalRewardId,
            GoalName = ag.GoalName,
            GoalDescription = ag.GoalDescription,
            GoalType = ag.GoalType,
            RewardType = ag.RewardType,
            CourseId = ag.CourseId,
            Progress = ag.Progress,
            IsCompleted = ag.IsCompleted,
            CompletedAt = ag.CompletedAt,
            RewardClaimed = ag.RewardClaimed,
            RewardClaimedAt = ag.RewardClaimedAt
        }).ToList();

        return result;
    }

    public async Task<List<Badge>> GetAllAsync()
    {
        return await _efDbContext.Badges.AsNoTracking().ToListAsync();
    }

    public async Task<Badge?> GetByIdAsync(int id)
    {
        return await _efDbContext.Badges.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<List<Badge?>> GetByUserIdAsync(int userId)
    {
        return _efDbContext.BadgeStudents.AsNoTracking()
            .Where(ub => ub.UserId == userId)
            .Select(ub => ub.Badge)
            .ToListAsync();
    }

    public async Task<List<GoalWithBadgesResponse>> GetGoalsWithBadgesByCourseIdAsync(int courseId)
    {
        var goalRewards = await _efDbContext.GoalRewards
            .AsNoTracking()
            .Where(g => g.CourseId == courseId)
            .Select(g => new
            {
                g.GoalId,
                g.Id,
                GoalName = g.Goal!.Name,
                GoalDescription = g.Goal!.Description,
                GoalType = g.Goal.Type,
                GoalImageUrl = g.Goal.ImageUrl,
                g.PointsReward,
                BadgeId = g.Badge!.Id,
                BadgeName = g.Badge.Name,
                BadgeDescription = g.Badge.Description,
                BadgeIconUrl = g.Badge.IconUrl
            })
            .ToListAsync();

        var goalsWithBadges = goalRewards
            .GroupBy(g => g.GoalId)
            .Select(g => new GoalWithBadgesResponse
            {
                GoalId = g.Key,
                GoalRewardId = g.Select(gr => gr.Id).FirstOrDefault(),
                GoalName = g.First().GoalName!,
                GoalDescription = g.First().GoalDescription!,
                GoalType = g.First().GoalType ?? GoalType.Custom,
                GoalImageUrl = g.First().GoalImageUrl!,
                Badges = g.Select(gr => new BadgeDTO
                {
                    Id = gr.BadgeId,
                    Name = gr.BadgeName!,
                    Description = gr.BadgeDescription!,
                    IconURL = gr.BadgeIconUrl!
                })
                .DistinctBy(b => b.Id)
                .ToList(),
                Points = g.Select(gr => gr.PointsReward).FirstOrDefault() ?? 0
            })
            .ToList();

        return goalsWithBadges;
    }

    public async Task<bool> UpdateActivatedGoalIdAsync(int goalRewardId, int userId)
    {
        var takeGoal = await _efDbContext.GoalRewards.FirstOrDefaultAsync(tg => tg.Id == goalRewardId);

        if (takeGoal == null)
            return false;

        var addGoalInStudents = new GoalStudent
        {
            GoalRewardId = takeGoal.Id,
            UserId = userId,
            CourseId = takeGoal.CourseId,
            Progress = 1,
            IsCompleted = false,
            CompletedAt = null
        };

        _efDbContext.GoalStudents.Add(addGoalInStudents);
        await _efDbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> GoalRewardOperationAsync(int goalRewardId, int userId)
    {
        var goalStudent = await _efDbContext.GoalStudents
            .FirstOrDefaultAsync(gs => gs.GoalRewardId == goalRewardId && gs.UserId == userId);

        if (goalStudent == null || !goalStudent.IsCompleted)
            return false;

        if (goalStudent.RewardClaimed)
            return false;

        var goalReward = await _efDbContext.GoalRewards
            .Include(gr => gr.Badge)
            .FirstOrDefaultAsync(gr => gr.Id == goalRewardId);

        if (goalReward == null)
            return false;

        // Badge - atribui todos os badges dos GoalRewards com o mesmo GoalId
        var goalRewardsWithSameGoal = await _efDbContext.GoalRewards
            .Include(gr => gr.Badge)
            .Where(gr => gr.GoalId == goalReward.GoalId && gr.Badge != null)
            .ToListAsync();

        foreach (var gr in goalRewardsWithSameGoal)
        {
            var alreadyHasBadge = await _efDbContext.BadgeStudents
                .AnyAsync(bs => bs.UserId == userId && bs.BadgeId == gr.Badge.Id);

            if (!alreadyHasBadge)
            {
                _efDbContext.BadgeStudents.Add(new BadgeStudent
                {
                    BadgeId = gr.Badge.Id,
                    UserId = userId,
                    AwardedAt = DateTime.UtcNow
                });
            }
        }

        //Pontos
        if (goalReward.PointsReward.HasValue && goalReward.PointsReward.Value > 0)
        {
            await _pointRepository.AddPointsAsync(new Point
            {
                UserId = userId,
                Points = float.Parse(goalReward.PointsReward.Value.ToString()),
                Reason = $"GoalReward:{goalReward.Id}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        //Marca como resgatado
        goalStudent.RewardClaimed = true;
        goalStudent.RewardClaimedAt = DateTime.UtcNow;

        await _efDbContext.SaveChangesAsync();

        return true;
    }
}
