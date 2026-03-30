using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class ProgressStudentPhaseRepository : IProgressStudentPhaseRepository
{
    private readonly AppDbContext _efDbContext;

    public ProgressStudentPhaseRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<ProgressStudentPhase>> GetAllAsync()
    {
        return await _efDbContext.ProgressStudentPhases.AsNoTracking().ToListAsync();
    }

    public async Task<ProgressStudentPhase?> GetByIdAsync(int id)
    {
        return await _efDbContext.ProgressStudentPhases.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ProgressStudentPhase?> GetProgressByUserIdAndPhaseIdAsync(int userId, int phaseId)
    {
        return await _efDbContext.ProgressStudentPhases.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.PhaseId == phaseId);
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
}
