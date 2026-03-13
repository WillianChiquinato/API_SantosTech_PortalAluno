using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API_PortalSantosTech.Repository;

public class ExerciseRepository : IExerciseRepository
{
    private readonly AppDbContext _efDbContext;

    public ExerciseRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<Exercise>> GetAllAsync()
    {
        return await _efDbContext.Exercises.AsNoTracking().ToListAsync();
    }

    public async Task<Exercise?> GetByIdAsync(int id)
    {
        return await _efDbContext.Exercises.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<ExerciseDailyTasksDTO>> GetDailyTasksForPhaseAsync(int phaseId, int userId)
    {
        var tasksContainer = await _efDbContext.ContainerTasks
            .Where(dt => dt.PhaseId == phaseId && dt.IsDailyTask)
            .AsNoTracking()
            .ToListAsync();

        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);

        var exerciseIds = tasksContainer
            .Select(task => task.ExerciseId)
            .Distinct()
            .ToList();

        var allPhaseExercises = await _efDbContext.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .AsNoTracking()
            .ToListAsync();

        var completedExerciseIds = await _efDbContext.Answers
            .Where(answer => answer.UserId == userId && exerciseIds.Contains(answer.ExerciseId))
            .Select(answer => answer.ExerciseId)
            .Distinct()
            .ToListAsync();

        var completedExerciseIdsSet = completedExerciseIds.ToHashSet();

        var overdueExercises = allPhaseExercises
            .Where(exercise => exercise.ExercisePeriod.ToUniversalTime().Date < todayUtc
                               && !completedExerciseIdsSet.Contains(exercise.Id))
            .ToList();

        var todayExercises = allPhaseExercises
            .Where(exercise => exercise.ExercisePeriod.ToUniversalTime().Date == todayUtc)
            .ToList();

        var todayPendingExercises = todayExercises
            .Where(exercise => !completedExerciseIdsSet.Contains(exercise.Id))
            .ToList();

        var tomorrowExercises = allPhaseExercises
            .Where(exercise => exercise.ExercisePeriod.ToUniversalTime().Date == tomorrowUtc)
            .ToList();

        var selectedExercises = overdueExercises.Count > 0
            ? overdueExercises.Concat(todayExercises).ToList()
            : todayExercises.Count > 0 && todayPendingExercises.Count == 0
                ? tomorrowExercises
                : todayExercises;

        var selectedExerciseIds = selectedExercises
            .Select(exercise => exercise.Id)
            .ToHashSet();

        tasksContainer = tasksContainer
            .Where(task => selectedExerciseIds.Contains(task.ExerciseId))
            .ToList();

        return tasksContainer
            .GroupBy(task => new { task.Name, task.PhaseId })
            .Select(group =>
            {
                return new ExerciseDailyTasksDTO
                {
                    Id = group.Min(task => task.Id),
                    Name = group.Key.Name ?? string.Empty,
                    PhaseId = group.Key.PhaseId,
                    Exercises = group
                        .Join(
                            selectedExercises,
                            task => task.ExerciseId,
                            exercise => exercise.Id,
                            (_, exercise) => new ExerciseDTO
                            {
                                Id = exercise.Id,
                                Title = exercise.Title,
                                Description = exercise.Description,
                                VideoUrl = exercise.VideoUrl,
                                PointsRedeem = exercise.PointsRedeem,
                                TermAt = exercise.TermAt,
                                TypeExercise = exercise.TypeExercise,
                                Difficulty = exercise.Difficulty,
                                IndexOrder = exercise.IndexOrder,
                                IsDailyTask = exercise.IsDailyTask,
                                IsFinalExercise = exercise.IsFinalExercise,
                                ExercisePeriod = exercise.ExercisePeriod
                            }
                        )
                        .OrderBy(exercise => exercise.IndexOrder)
                        .ToList()
                };
            })
            .OrderBy(taskGroup => taskGroup.Id)
            .ToList();
    }

    public Task<List<ExerciseAnswerDTO>> GetDailyExercisesAnswersForPhaseAsync(int phaseId, int userId)
    {
        var exerciseIds = _efDbContext.ContainerTasks
            .Where(dt => dt.PhaseId == phaseId)
            .Select(dt => dt.ExerciseId)
            .Distinct()
            .ToList();

        return _efDbContext.Answers
            .Where(a => exerciseIds.Contains(a.ExerciseId) && a.UserId == userId)
            .AsNoTracking()
            .Select(a => new ExerciseAnswerDTO
            {
                Id = a.Id,
                QuestionId = a.QuestionId,
                ExerciseId = a.ExerciseId,
                UserId = a.UserId,
                IsCorrect = a.IsCorrect,
                Answer = a.AnswerText ?? string.Empty,
                SubmittedAt = a.AnsweredAt
            })
            .ToListAsync();
    }

    public Task<List<ExerciseDTO>> GetExercisesByPhaseId(int phaseId)
    {
        return _efDbContext.Exercises
            .Where(e => e.PhaseId == phaseId && !e.IsDailyTask)
            .AsNoTracking()
            .Select(e => new ExerciseDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                VideoUrl = e.VideoUrl,
                PointsRedeem = e.PointsRedeem,
                TermAt = e.TermAt,
                TypeExercise = e.TypeExercise,
                Difficulty = e.Difficulty,
                IndexOrder = e.IndexOrder,
                IsDailyTask = e.IsDailyTask,
                IsFinalExercise = e.IsFinalExercise,
                ExercisePeriod = e.ExercisePeriod
            })
            .ToListAsync();
    }

    public async Task<List<QuestionOptionsDTO>> GetQuestionsOptionsForExerciseAsync(int exerciseId)
    {
        var questions = await _efDbContext.Questions
            .Where(q => q.ExerciseId == exerciseId)
            .AsNoTracking()
            .ToListAsync();

        var questionIds = questions.Select(q => q.Id).ToList();

        return await _efDbContext.QuestionOptions
            .Where(qo => questionIds.Contains(qo.QuestionId))
            .AsNoTracking()
            .Select(qo => new QuestionOptionsDTO
            {
                Id = qo.Id,
                QuestionId = qo.QuestionId,
                Question = qo.OptionText,
                CorrectAnswer = qo.IsCorrect
            })
            .ToListAsync();
    }

    public async Task<bool> SubmitExerciseAnswersAsync(ExerciseSubmissionDTO submission)
    {
        await _efDbContext.Answers.AddAsync(new Answer
        {
            UserId = submission.UserId,
            QuestionId = submission.QuestionId,
            UserExerciseFlowId = submission.UserExerciseFlowId,
            ExerciseId = submission.ExerciseId,
            AnswerText = submission.SubmissionData!.AnswerText,
            SelectedOption = submission.SubmissionData!.SelectedOption,
            IsCorrect = submission.SubmissionData.IsCorrect,
            AnsweredAt = DateTime.UtcNow
        });

        await _efDbContext.SaveChangesAsync();
        return true;
    }

    public Task<List<ExerciseAnswerDTO>> GetExercisesAnswersForUserAsync(int userId)
    {
        return _efDbContext.Answers
            .Where(a => a.UserId == userId)
            .AsNoTracking()
            .Select(a => new ExerciseAnswerDTO
            {
                Id = a.Id,
                QuestionId = a.QuestionId,
                ExerciseId = a.ExerciseId,
                UserId = a.UserId,
                UserExerciseFlowId = a.UserExerciseFlowId ?? 0,
                IsCorrect = a.IsCorrect,
                Answer = a.AnswerText ?? string.Empty,
                SubmittedAt = a.AnsweredAt
            })
            .ToListAsync();
    }

    public Task<List<ExerciseAnswerDTO>> GetExercisesAnswersForUserByExerciseIdsAsync(int userId, List<int> exerciseIds)
    {
        return _efDbContext.Answers
            .Where(a => a.UserId == userId && exerciseIds.Contains(a.ExerciseId))
            .AsNoTracking()
            .Select(a => new ExerciseAnswerDTO
            {
                Id = a.Id,
                QuestionId = a.QuestionId,
                ExerciseId = a.ExerciseId,
                UserId = a.UserId,
                IsCorrect = a.IsCorrect,
                Answer = a.AnswerText ?? string.Empty,
                SubmittedAt = a.AnsweredAt
            })
            .ToListAsync();
    }

    public Task<List<UserExerciseFlow>> GetByUserAndPhaseOrderedAsync(int userId, int phaseId)
    {
        return _efDbContext.UserExerciseFlows
            .Where(uef => uef.UserId == userId && uef.PhaseId == phaseId)
            .AsNoTracking()
            .OrderBy(uef => uef.IndexOrder)
            .ToListAsync();
    }

    public Task<List<Exercise>> GetByIdsAsync(List<int> ids)
    {
        return _efDbContext.Exercises
            .Where(e => ids.Contains(e.Id))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task CreateUserExerciseFlowAsync(List<UserExerciseFlow> userExerciseFlows)
    {
        await _efDbContext.UserExerciseFlows.AddRangeAsync(userExerciseFlows);
        await _efDbContext.SaveChangesAsync();

        if (userExerciseFlows.Any())
        {
            await EnsureTypeThreeExercisesAtEndAsync(
                userExerciseFlows.First().UserId,
                userExerciseFlows.First().PhaseId);
        }
    }

    public async Task<List<Exercise>> GetFlowWithExercisesAsync(int userId, int phaseId)
    {
        var userExerciseFlows = await _efDbContext.UserExerciseFlows
            .Where(uef => uef.UserId == userId && uef.PhaseId == phaseId)
            .Include(uef => uef.Exercise)
            .OrderBy(uef => uef.IndexOrder)
            .AsNoTracking()
            .ToListAsync();

        return userExerciseFlows
            .Where(uef => uef.Exercise != null)
            .Select(uef => uef.Exercise!)
            .ToList();
    }

    public Task<List<ContainerTask>> GetNonDailyContainerTasksByPhaseAsync(int phaseId)
    {
        return _efDbContext.ContainerTasks
            .Where(ct => ct.PhaseId == phaseId && !ct.IsDailyTask)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task InsertLowerExercisesAsync(int userId, int phaseId, int mainExerciseId, int? userExerciseFlowId)
    {
        var alreadyInserted = await _efDbContext.UserExerciseFlows
            .AnyAsync(f =>
                f.UserId == userId &&
                f.PhaseId == phaseId &&
                f.TriggeredByExerciseId == mainExerciseId);

        if (alreadyInserted)
            return;

        var lowerPool = await _efDbContext.Exercises
            .Where(e =>
                e.PhaseId == phaseId &&
                e.Difficulty == DifficultyLevel.Lower)
            .AsNoTracking()
            .ToListAsync();

        if (!lowerPool.Any())
            return;

        var random = new Random();
        var quantity = random.Next(1, 4);

        var selectedLowers = lowerPool
            .OrderBy(_ => Guid.NewGuid())
            .Take(quantity)
            .ToList();

        UserExerciseFlow? mainFlow = null;

        if (userExerciseFlowId.HasValue)
        {
            mainFlow = await _efDbContext.UserExerciseFlows
                .FirstOrDefaultAsync(f =>
                    f.Id == userExerciseFlowId.Value &&
                    f.UserId == userId &&
                    f.PhaseId == phaseId);
        }

        if (mainFlow == null)
        {
            mainFlow = await _efDbContext.UserExerciseFlows
                .FirstOrDefaultAsync(f =>
                    f.UserId == userId &&
                    f.PhaseId == phaseId &&
                    f.ExerciseId == mainExerciseId);
        }

        if (mainFlow == null)
            return;

        var insertionIndex = mainFlow.IndexOrder + 1;

        var flowsToShift = await _efDbContext.UserExerciseFlows
            .Where(f =>
                f.UserId == userId &&
                f.PhaseId == phaseId &&
                f.IndexOrder >= insertionIndex)
            .ToListAsync();

        foreach (var flow in flowsToShift)
        {
            flow.IndexOrder += quantity;
        }

        int localIndex = insertionIndex;

        var newFlows = selectedLowers.Select(lower => new UserExerciseFlow
        {
            UserId = userId,
            PhaseId = phaseId,
            ExerciseId = lower.Id,
            IndexOrder = localIndex++,
            Origin = FlowOrigin.Lower,
            TriggeredByExerciseId = mainExerciseId,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await _efDbContext.UserExerciseFlows.AddRangeAsync(newFlows);

        //Salvar tudo junto
        await _efDbContext.SaveChangesAsync();

        await EnsureTypeThreeExercisesAtEndAsync(userId, phaseId);
    }

    public async Task<int> SyncMainExercisesIntoExistingFlowsAsync(int phaseId)
    {
        var orderedMainExerciseIds = await GetOrderedMainExerciseIdsByContainerAsync(phaseId);

        if (!orderedMainExerciseIds.Any())
            return 0;

        var userIds = await _efDbContext.UserExerciseFlows
            .Where(f => f.PhaseId == phaseId)
            .Select(f => f.UserId)
            .Distinct()
            .ToListAsync();

        if (!userIds.Any())
            return 0;

        var userMainFlowRows = await _efDbContext.UserExerciseFlows
            .Where(f => f.PhaseId == phaseId && f.Origin == FlowOrigin.Main)
            .Select(f => new { f.UserId, f.ExerciseId })
            .ToListAsync();

        var maxOrderByUser = await _efDbContext.UserExerciseFlows
            .Where(f => f.PhaseId == phaseId)
            .GroupBy(f => f.UserId)
            .Select(g => new { UserId = g.Key, MaxOrder = g.Max(x => x.IndexOrder) })
            .ToDictionaryAsync(x => x.UserId, x => x.MaxOrder);

        var existingMainByUser = userMainFlowRows
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ExerciseId).ToHashSet());

        var now = DateTime.UtcNow;
        var flowsToInsert = new List<UserExerciseFlow>();
        var touchedUsers = new HashSet<int>();

        foreach (var userId in userIds)
        {
            var existingMain = existingMainByUser.GetValueOrDefault(userId, new HashSet<int>());

            var missingMainIds = orderedMainExerciseIds
                .Where(exerciseId => !existingMain.Contains(exerciseId))
                .ToList();

            if (!missingMainIds.Any())
                continue;

            var order = maxOrderByUser.GetValueOrDefault(userId, -1) + 1;

            foreach (var exerciseId in missingMainIds)
            {
                flowsToInsert.Add(new UserExerciseFlow
                {
                    UserId = userId,
                    PhaseId = phaseId,
                    ExerciseId = exerciseId,
                    IndexOrder = order++,
                    Origin = FlowOrigin.Main,
                    TriggeredByExerciseId = null,
                    CreatedAt = now
                });
            }

            touchedUsers.Add(userId);
        }

        if (!flowsToInsert.Any())
            return 0;

        await _efDbContext.UserExerciseFlows.AddRangeAsync(flowsToInsert);
        await _efDbContext.SaveChangesAsync();

        foreach (var userId in touchedUsers)
        {
            await EnsureTypeThreeExercisesAtEndAsync(userId, phaseId);
        }

        return flowsToInsert.Count;
    }

    public async Task<int> SyncMainExercisesForUserPhaseAsync(int userId, int phaseId)
    {
        var orderedMainExerciseIds = await GetOrderedMainExerciseIdsByContainerAsync(phaseId);

        if (!orderedMainExerciseIds.Any())
            return 0;

        var existingMainIds = await _efDbContext.UserExerciseFlows
            .Where(f => f.UserId == userId && f.PhaseId == phaseId && f.Origin == FlowOrigin.Main)
            .Select(f => f.ExerciseId)
            .ToListAsync();

        var existingMainSet = existingMainIds.ToHashSet();

        var missingMainIds = orderedMainExerciseIds
            .Where(exerciseId => !existingMainSet.Contains(exerciseId))
            .ToList();

        if (!missingMainIds.Any())
            return 0;

        var maxOrder = await _efDbContext.UserExerciseFlows
            .Where(f => f.UserId == userId && f.PhaseId == phaseId)
            .Select(f => (int?)f.IndexOrder)
            .MaxAsync() ?? -1;

        var order = maxOrder + 1;
        var now = DateTime.UtcNow;

        var flowsToInsert = missingMainIds
            .Select(exerciseId => new UserExerciseFlow
            {
                UserId = userId,
                PhaseId = phaseId,
                ExerciseId = exerciseId,
                IndexOrder = order++,
                Origin = FlowOrigin.Main,
                TriggeredByExerciseId = null,
                CreatedAt = now
            })
            .ToList();

        await _efDbContext.UserExerciseFlows.AddRangeAsync(flowsToInsert);
        await _efDbContext.SaveChangesAsync();

        await EnsureTypeThreeExercisesAtEndAsync(userId, phaseId);

        return flowsToInsert.Count;
    }

    private async Task EnsureTypeThreeExercisesAtEndAsync(int userId, int phaseId)
    {
        var orderedFlow = await _efDbContext.UserExerciseFlows
            .Where(f => f.UserId == userId && f.PhaseId == phaseId)
            .Include(f => f.Exercise)
            .OrderBy(f => f.Exercise != null && f.Exercise.Difficulty == DifficultyLevel.ProofTest ? 1 : 0)
            .ThenBy(f => f.IndexOrder)
            .ToListAsync();

        var hasChanges = false;

        for (int i = 0; i < orderedFlow.Count; i++)
        {
            if (orderedFlow[i].IndexOrder == i)
                continue;

            orderedFlow[i].IndexOrder = i;
            hasChanges = true;
        }

        if (hasChanges)
            await _efDbContext.SaveChangesAsync();
    }

    private async Task<List<int>> GetOrderedMainExerciseIdsByContainerAsync(int phaseId)
    {
        var phaseMainExercises = await _efDbContext.Exercises
            .Where(e => e.PhaseId == phaseId && !e.IsDailyTask && e.Difficulty != DifficultyLevel.Lower)
            .AsNoTracking()
            .ToListAsync();

        if (!phaseMainExercises.Any())
            return new List<int>();

        var containerRows = await _efDbContext.ContainerTasks
            .Where(ct => ct.PhaseId == phaseId && !ct.IsDailyTask)
            .Include(ct => ct.Exercise)
            .AsNoTracking()
            .ToListAsync();

        var containerOrderedIds = containerRows
            .Where(ct => ct.Exercise != null && ct.Exercise.Difficulty != DifficultyLevel.Lower)
            .GroupBy(ct => ct.Name ?? string.Empty)
            .OrderBy(g => g.Min(x => x.ContainerDateTargetInt))
            .ThenBy(g => g.Min(x => x.Id))
            .SelectMany(g => g
                .OrderBy(x => x.Exercise!.IndexOrder)
                .ThenBy(x => x.ExerciseId)
                .Select(x => x.ExerciseId))
            .Distinct()
            .ToList();

        var containerIdSet = containerOrderedIds.ToHashSet();

        var standaloneOrderedIds = phaseMainExercises
            .Where(e => !containerIdSet.Contains(e.Id))
            .OrderBy(e => e.Difficulty == DifficultyLevel.ProofTest ? 1 : 0)
            .ThenBy(e => e.IndexOrder)
            .ThenBy(e => e.Id)
            .Select(e => e.Id)
            .ToList();

        return containerOrderedIds
            .Concat(standaloneOrderedIds)
            .Distinct()
            .ToList();
    }

    public async Task<bool> VerifyExistingAnswersAsync(int exerciseId, int userId)
    {
        return await _efDbContext.Answers
            .AnyAsync(a => a.ExerciseId == exerciseId && a.UserId == userId);
    }

    public Task<List<ContainerTask>> GetContainerTasksByPhaseIdAsync(int phaseId)
    {
        return _efDbContext.ContainerTasks
            .Where(ct => ct.PhaseId == phaseId)
            .Include(ct => ct.Exercise)
            .AsNoTracking()
            .ToListAsync();
    }
}
