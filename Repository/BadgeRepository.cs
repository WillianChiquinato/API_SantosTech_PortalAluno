using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class BadgeRepository : IBadgeRepository
{
    private readonly AppDbContext _efDbContext;

    public BadgeRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<ActivatedGoalResponse>> GetActivatedGoalsByUserIdAsync(int userId)
    {
        var activatedGoals = await _efDbContext.GoalStudents
            .AsNoTracking()
            .Where(gs => gs.UserId == userId)
            .Select(gs => new ActivatedGoalResponse
            {
                Id = gs.Id,
                UserId = gs.UserId,
                GoalId = gs.GoalId,
                CourseId = gs.CourseId,
                Progress = gs.Progress,
                IsCompleted = gs.IsCompleted,
                CompletedAt = gs.CompletedAt
            })
            .ToListAsync();

        return activatedGoals;
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
                GoalName = g.Goal!.Name,
                GoalDescription = g.Goal!.Description,
                GoalType = g.Goal.Type,
                GoalImageUrl = g.Goal.ImageUrl,
                g.Points,
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
                Points = g.Select(gr => gr.Points).FirstOrDefault() ?? 0
            })
            .ToList();

        return goalsWithBadges;
    }

    public async Task<bool> UpdateActivatedGoalIdAsync(int goalId, int userId)
    {
        var takeGoal = await _efDbContext.GoalRewards.FirstOrDefaultAsync(tg => tg.GoalId == goalId);

        if (takeGoal == null)
            return false;

        var addGoalInStudents = new GoalStudent
        {
            GoalId = goalId,
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
}
