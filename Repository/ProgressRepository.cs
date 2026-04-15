using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class ProgressRepository : IProgressRepository
{
    private readonly AppDbContext _efDbContext;

    private static bool TryResolveGoalType(int rawGoalType, out GoalType goalType)
    {
        if (Enum.IsDefined(typeof(GoalType), rawGoalType))
        {
            goalType = (GoalType)rawGoalType;
            return true;
        }

        // Accept 0-based values from clients and map them to the persisted enum values.
        var oneBasedGoalType = rawGoalType + 1;
        if (Enum.IsDefined(typeof(GoalType), oneBasedGoalType))
        {
            goalType = (GoalType)oneBasedGoalType;
            return true;
        }

        goalType = default;
        return false;
    }

    private static bool TryResolveRewardType(int rawRewardType, out RewardType rewardType)
    {
        if (Enum.IsDefined(typeof(RewardType), rawRewardType))
        {
            rewardType = (RewardType)rawRewardType;
            return true;
        }

        // Accept 0-based values from clients and map them to the persisted enum values.
        var oneBasedRewardType = rawRewardType + 1;
        if (Enum.IsDefined(typeof(RewardType), oneBasedRewardType))
        {
            rewardType = (RewardType)oneBasedRewardType;
            return true;
        }

        rewardType = default;
        return false;
    }

    private static int[] GetRewardTypeQueryValues(RewardType rewardType)
    {
        var currentValue = (int)rewardType;

        // Keep compatibility with legacy rows that may have persisted enum values as 0-based.
        return currentValue > 0
            ? new[] { currentValue, currentValue - 1 }
            : new[] { currentValue };
    }

    public ProgressRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<VideoProgressDTO> AddProgressVideoAsync(VideoProgressDTO progressData)
    {
        var newProgress = new ProgressVideoStudent
        {
            UserId = progressData.UserId,
            VideoId = progressData.VideoId,
            WatchedSeconds = progressData.WatchSeconds,
            IsCompleted = progressData.IsCompleted,
            LastWatchedAt = DateTime.UtcNow
        };

        _efDbContext.ProgressVideoStudents.Add(newProgress);
        await _efDbContext.SaveChangesAsync();

        return new VideoProgressDTO
        {
            UserId = newProgress.UserId,
            VideoId = newProgress.VideoId,
            WatchSeconds = newProgress.WatchedSeconds,
            IsCompleted = newProgress.IsCompleted,
            LastWatched = newProgress.LastWatchedAt ?? DateTime.MinValue
        };
    }

    public async Task<List<ProgressStudentPhase>> GetAllAsync()
    {
        return await _efDbContext.ProgressStudentPhases.AsNoTracking().ToListAsync();
    }

    public async Task<List<ProgressExerciseStudent>> GetAllExerciseAsync()
    {
        return await _efDbContext.ProgressExerciseStudents.AsNoTracking().ToListAsync();
    }

    public async Task<List<ProgressStudentPhase>> GetAllStudentPhasesAsync()
    {
        return await _efDbContext.ProgressStudentPhases.AsNoTracking().ToListAsync();
    }

    public async Task<List<ProgressVideoStudent>> GetAllVideoStudentsAsync()
    {
        return await _efDbContext.ProgressVideoStudents.AsNoTracking().ToListAsync();
    }

    public async Task<ProgressStudentPhase?> GetStudentPhaseByIdAsync(int id)
    {
        return await _efDbContext.ProgressStudentPhases.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ProgressExerciseStudent?> GetExerciseByIdAsync(int id)
    {
        return await _efDbContext.ProgressExerciseStudents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ProgressStudentPhase?> GetProgressByUserIdAndPhaseIdAsync(int userId, int phaseId)
    {
        return await _efDbContext.ProgressStudentPhases.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.PhaseId == phaseId);
    }

    public async Task<List<ProgressPaidCourses>> GetProgressUserPaidCoursesAsync(int userId)
    {
        return await _efDbContext.ProgressPaidCourses.AsNoTracking()
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<VideoProgressDTO>> GetProgressUserVideosAsync(int userId)
    {
        return await _efDbContext.ProgressVideoStudents
            .Where(vp => vp.UserId == userId)
            .Select(vp => new VideoProgressDTO
            {
                VideoId = vp.VideoId,
                UserId = vp.UserId,
                WatchSeconds = vp.WatchedSeconds,
                IsCompleted = vp.IsCompleted,
                LastWatched = vp.LastWatchedAt ?? DateTime.MinValue
            })
            .ToListAsync();
    }

    public async Task<ProgressVideoStudent?> GetVideoStudentByIdAsync(int id)
    {
        return await _efDbContext.ProgressVideoStudents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ProgressGoalStudent?> UpdateGoalProgressAsync(int userId, int goalType, int rewardType)
    {
        if (!TryResolveGoalType(goalType, out var goalTypeEnum) ||
            !TryResolveRewardType(rewardType, out var rewardTypeEnum))
        {
            return null;
        }

        var rewardTypeQueryValues = GetRewardTypeQueryValues(rewardTypeEnum);

        var goalReward = await _efDbContext.GoalRewards
            .AsNoTracking()
            .FirstOrDefaultAsync(gr => gr.Goal!.Type == goalTypeEnum && rewardTypeQueryValues.Contains((int)gr.RewardType));

        if (goalReward == null)
        {
            return null;
        }

        var goalStudent = new GoalStudent
        {
            UserId = userId,
            GoalRewardId = goalReward.Id,
            CourseId = goalReward.CourseId,
            Progress = 0,
            IsCompleted = false
        };

        _efDbContext.GoalStudents.Add(goalStudent);
        await _efDbContext.SaveChangesAsync();

        return new ProgressGoalStudent
        {
            GoalStudentId = goalStudent.Id,
            UserId = goalStudent.UserId,
            GoalType = (int)(goalReward.Goal?.Type ?? goalTypeEnum),
            RewardType = (int)goalReward.RewardType,
            ProgressValue = goalStudent.Progress
        };
    }

    public async Task<bool> UpdateProgressAsync(int userId, int phaseId, int totalAnswered, int totalExercises)
    {
        var userProgressCount = await _efDbContext.ProgressStudentPhases
            .CountAsync(x => x.UserId == userId);

        var progress = await _efDbContext.ProgressStudentPhases
            .FirstOrDefaultAsync(x => x.UserId == userId && x.PhaseId == phaseId);

        var calculatedProgress = totalExercises > 0 && totalAnswered > 0
            ? (totalAnswered * 100d) / totalExercises
            : 1;

        if (progress == null)
        {
            var isFirstProgressForUser = userProgressCount == 0;

            progress = new ProgressStudentPhase
            {
                UserId = userId,
                PhaseId = phaseId,
                Status = isFirstProgressForUser
                    ? StatusStates.Unlocked
                    : totalAnswered <= 0
                        ? StatusStates.Pending
                        : totalAnswered == totalExercises
                            ? StatusStates.Completed
                            : StatusStates.Unlocked,
                Progress = calculatedProgress
            };
            _efDbContext.ProgressStudentPhases.Add(progress);
        }
        else if (userProgressCount == 1 && progress.Status == StatusStates.Unlocked)
        {
            // Keep the first record unlocked and only advance its progress.
            progress.Progress = calculatedProgress;
            _efDbContext.ProgressStudentPhases.Update(progress);
        }
        else
        {
            var shouldComplete = totalExercises > 0 && totalAnswered >= totalExercises;
            var shouldUnlock = totalAnswered > 0 && !shouldComplete;
            var shouldKeepCurrentStatus = totalAnswered <= 0 &&
                                          (progress.Status == StatusStates.Unlocked || progress.Status == StatusStates.Completed);

            progress.Status = shouldComplete
                ? StatusStates.Completed
                : shouldUnlock
                    ? StatusStates.Unlocked
                    : shouldKeepCurrentStatus
                        ? progress.Status
                        : StatusStates.Pending;
            progress.Progress = calculatedProgress;
            _efDbContext.ProgressStudentPhases.Update(progress);
        }

        await _efDbContext.SaveChangesAsync();

        // If the current phase just became Completed, unlock the next phase
        var updatedProgress = await _efDbContext.ProgressStudentPhases
            .FirstOrDefaultAsync(x => x.UserId == userId && x.PhaseId == phaseId);

        if (updatedProgress?.Status == StatusStates.Completed)
        {
            var currentPhase = await _efDbContext.Phases
                .FirstOrDefaultAsync(x => x.Id == phaseId);

            if (currentPhase != null)
            {
                var nextPhase = await _efDbContext.Phases
                    .FirstOrDefaultAsync(x => x.ModuleId == currentPhase.ModuleId &&
                                              x.IndexOrder == currentPhase.IndexOrder + 1);

                if (nextPhase != null)
                {
                    var nextPhaseProgress = await _efDbContext.ProgressStudentPhases
                        .FirstOrDefaultAsync(x => x.UserId == userId && x.PhaseId == nextPhase.Id);

                    if (nextPhaseProgress == null)
                    {
                        nextPhaseProgress = new ProgressStudentPhase
                        {
                            UserId = userId,
                            PhaseId = nextPhase.Id,
                            Status = StatusStates.Unlocked,
                            Progress = 1
                        };
                        _efDbContext.ProgressStudentPhases.Add(nextPhaseProgress);
                    }
                    else if (nextPhaseProgress.Status == StatusStates.Pending)
                    {
                        nextPhaseProgress.Status = StatusStates.Unlocked;
                        _efDbContext.ProgressStudentPhases.Update(nextPhaseProgress);
                    }

                    await _efDbContext.SaveChangesAsync();
                }
            }
        }

        return true;
    }

    public async Task<VideoProgressDTO> UpdateProgressVideoAsync(VideoProgressDTO progressData)
    {
        var existingProgress = await _efDbContext.ProgressVideoStudents
            .FirstOrDefaultAsync(p => p.UserId == progressData.UserId && p.VideoId == progressData.VideoId);

        if (existingProgress != null)
        {
            existingProgress.WatchedSeconds = progressData.WatchSeconds;
            existingProgress.IsCompleted = progressData.IsCompleted;
            existingProgress.LastWatchedAt = DateTime.UtcNow;

            await _efDbContext.SaveChangesAsync();
        }

        return new VideoProgressDTO
        {
            UserId = existingProgress?.UserId ?? progressData.UserId,
            VideoId = existingProgress?.VideoId ?? progressData.VideoId,
            WatchSeconds = existingProgress?.WatchedSeconds ?? progressData.WatchSeconds,
            IsCompleted = existingProgress?.IsCompleted ?? progressData.IsCompleted,
            LastWatched = existingProgress?.LastWatchedAt ?? DateTime.MinValue
        };
    }

    public async Task<bool> EvaluateProgress(int userId, int goalRewardId, int rewardType)
    {
        if (!TryResolveRewardType(rewardType, out var rewardTypeEnum))
        {
            return false;
        }

        var rewardTypeQueryValues = GetRewardTypeQueryValues(rewardTypeEnum);

        var progress = await _efDbContext.GoalStudents
            .Include(x => x.GoalReward)
            .ThenInclude(gr => gr!.Goal)
            .FirstOrDefaultAsync(x => x.GoalRewardId == goalRewardId && x.GoalReward != null);

        if (progress?.GoalReward == null || !rewardTypeQueryValues.Contains((int)progress.GoalReward.RewardType))
        {
            return false;
        }

        if (progress?.GoalReward?.Goal?.Type == null)
        {
            return false;
        }

        //Apenas a regra de metas por pontos. 
        switch (progress.GoalReward?.Goal!.Type)
        {
            case GoalType.PointQuantity:
                await PointsQuantityRule(progress, rewardTypeEnum);
                break;
            default:
                return false;
        }

        return true;
    }

    public async Task<bool> PointsQuantityRule(GoalStudent progress, RewardType rewardTypeEnum)
    {
        var resultGoalReward = await _efDbContext.GoalRewards
                    .AsNoTracking()
                    .FirstOrDefaultAsync(gr => gr.Id == progress.GoalRewardId);

        if (resultGoalReward?.PointsReward is null || resultGoalReward.PointsReward <= 0 ||
            resultGoalReward.PointsTarget is null || resultGoalReward.PointsTarget <= 0)
        {
            return false;
        }

        var pointsTarget = resultGoalReward.PointsTarget.Value;

        double totalPoints;

        if (rewardTypeEnum == RewardType.PointsBasic)
        {
            totalPoints = await _efDbContext.Points
                .Where(p => p.UserId == progress.UserId)
                .SumAsync(p => p.Points);

            if (totalPoints >= pointsTarget)
            {
                progress.IsCompleted = true;
                progress.Progress = 100;
                progress.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                progress.Progress = (totalPoints * 100) / pointsTarget;
            }
        }
        else if (rewardTypeEnum == RewardType.PointsBetweenDates)
        {
            var DateRange = resultGoalReward.StartDateTarget.HasValue && resultGoalReward.EndDateTarget.HasValue
                ? (StartDate: resultGoalReward.StartDateTarget.Value, EndDate: resultGoalReward.EndDateTarget.Value)
                : (StartDate: DateTime.UtcNow.AddDays(-30), EndDate: DateTime.UtcNow);

            totalPoints = await _efDbContext.Points
                .Where(p => p.UserId == progress.UserId
                            && p.CreatedAt >= DateRange.StartDate
                            && p.CreatedAt <= DateRange.EndDate)
                .SumAsync(p => p.Points);
        }
        else
        {
            return false;
        }

        if (totalPoints >= pointsTarget)
        {
            progress.IsCompleted = true;
            progress.Progress = 100;
            progress.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            progress.Progress = (totalPoints * 100) / pointsTarget;
        }

        _efDbContext.GoalStudents.Update(progress);
        await _efDbContext.SaveChangesAsync();
        return true;
    }
}
