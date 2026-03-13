using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class ClassService : IClassService
{
    private readonly ILogger<ClassService> _logger;
    private readonly IClassRepository _classRepository;
    private readonly IProgressStudentPhaseRepository _progressStudentPhaseRepository;
    private readonly IExerciseRepository _exerciseRepository;

    public ClassService(ILogger<ClassService> logger, IClassRepository classRepository, IProgressStudentPhaseRepository progressStudentPhaseRepository, IExerciseRepository exerciseRepository)
    {
        _logger = logger;
        _classRepository = classRepository;
        _progressStudentPhaseRepository = progressStudentPhaseRepository;
        _exerciseRepository = exerciseRepository;
    }

    public async Task<CustomResponse<IEnumerable<Class>>> GetAllAsync()
    {
        try
        {
            var result = await _classRepository.GetAllAsync();
            return CustomResponse<IEnumerable<Class>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todas as classes");
            return CustomResponse<IEnumerable<Class>>.Fail("Ocorreu um erro ao buscar as classes");
        }
    }

    public async Task<CustomResponse<Class>> GetByIdAsync(int id)
    {
        try
        {
            var result = await _classRepository.GetByIdAsync(id);
            if (result == null)
                return CustomResponse<Class>.Fail("Classe não encontrada");

            return CustomResponse<Class>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar a classe com ID {ClassId}", id);
            return CustomResponse<Class>.Fail("Ocorreu um erro ao buscar a classe");
        }
    }

    public async Task<CustomResponse<IEnumerable<IslandDTO>>> GetIslandsByUserIdAndCurrentModuleAsync(int userId, int phaseId)
    {
        try
        {
            var module = await _classRepository.GetModuleByPhaseIdAsync(phaseId);
            if (module == null)
                return CustomResponse<IEnumerable<IslandDTO>>.Fail("Módulo não encontrado para a fase informada");

            var phases = await _classRepository.GetPhasesByCurrentModuleAsync(module.Id);

            if (phases == null || !phases.Any())
                return CustomResponse<IEnumerable<IslandDTO>>.Fail("Nenhuma ilha encontrada para o módulo");

            var userAnswers = await _exerciseRepository
                .GetExercisesAnswersForUserAsync(userId);

            var finalResult = new List<IslandDTO>();

            //Sobre o bloqueio por dias.
            var userCourse = await _classRepository.GetByIdAsync(module.CourseId);

            var startDate = userCourse!.StartDate.Date;
            var today = DateTime.UtcNow.Date;

            foreach (var phase in phases)
            {
                var statusProgress = await _progressStudentPhaseRepository
                    .GetProgressByUserIdAndPhaseIdAsync(userId, phase.Id);

                await _exerciseRepository
                    .SyncMainExercisesForUserPhaseAsync(userId, phase.Id);

                var flow = await _exerciseRepository
                        .GetByUserAndPhaseOrderedAsync(userId, phase.Id);

                if (!flow.Any())
                {
                    await CreateInitialFlowAsync(userId, phase.Id);

                    flow = await _exerciseRepository
                        .GetByUserAndPhaseOrderedAsync(userId, phase.Id);
                }

                var flowExercises = await _exerciseRepository
                    .GetFlowWithExercisesAsync(userId, phase.Id);

                var nonDailyContainerTasks = await _exerciseRepository
                    .GetNonDailyContainerTasksByPhaseAsync(phase.Id);

                var exercisesById = flowExercises
                    .DistinctBy(e => e.Id)
                    .ToDictionary(e => e.Id);

                var flowByExerciseId = flow
                    .GroupBy(f => f.ExerciseId)
                    .ToDictionary(g => g.Key, g => g.OrderBy(x => x.IndexOrder).First());

                var flowOrderById = flow
                    .ToDictionary(item => item.Id, item => item.IndexOrder);

                var flowIds = flow.Select(f => f.Id).ToList();
                var phaseAnswers = userAnswers
                    .Where(a => flowIds.Contains(a.UserExerciseFlowId))
                    .ToList();

                var latestAnswers = phaseAnswers
                    .GroupBy(a => a.UserExerciseFlowId)
                    .Select(g => g.OrderByDescending(x => x.SubmittedAt).First())
                    .ToList();

                var answersByFlowId = latestAnswers
                    .ToDictionary(a => a.UserExerciseFlowId);

                // Lower exercises agrupados pelo exercício que os originou
                var lowerFlowByTriggeredId = flow
                    .Where(f => f.Origin == FlowOrigin.Lower && f.TriggeredByExerciseId.HasValue)
                    .GroupBy(f => f.TriggeredByExerciseId!.Value)
                    .ToDictionary(g => g.Key, g => g.OrderBy(x => x.IndexOrder).ToList());

                // ExerciseIds que pertencem a algum container (para filtrar o fallback)
                var containerTaskExerciseIdSet = nonDailyContainerTasks
                    .Select(t => t.ExerciseId)
                    .ToHashSet();

                var blipsList = nonDailyContainerTasks
                    .GroupBy(task => new { task.Name, task.PhaseId })
                    .Select(group =>
                    {
                        var flowItems = group
                            .Select(task => flowByExerciseId.GetValueOrDefault(task.ExerciseId))
                            .Where(flowItem => flowItem != null)
                            .Cast<UserExerciseFlow>()
                            .OrderBy(flowItem => flowItem.IndexOrder)
                            .ToList();

                        if (!flowItems.Any())
                            return null;

                        // Lower exercises cujo TriggeredByExerciseId pertence a este container
                        var groupExerciseIds = group.Select(task => task.ExerciseId).ToHashSet();
                        var lowerItems = groupExerciseIds
                            .SelectMany(eid => lowerFlowByTriggeredId.GetValueOrDefault(eid, new List<UserExerciseFlow>()))
                            .OrderBy(f => f.IndexOrder)
                            .ToList();

                        var allFlowItems = flowItems.Concat(lowerItems)
                            .OrderBy(f => f.IndexOrder)
                            .ToList();

                        var firstFlowItem = allFlowItems.First();
                        var allAnswered = allFlowItems.All(flowItem => answersByFlowId.ContainsKey(flowItem.Id));

                        var containerState = allAnswered
                            ? "Concluído"
                            : "Não iniciado";

                        var containerExercises = allFlowItems
                            .Select(flowItem =>
                            {
                                if (!exercisesById.TryGetValue(flowItem.ExerciseId, out var exercise))
                                    return null;

                                var exerciseState = answersByFlowId.TryGetValue(flowItem.Id, out var answer)
                                    ? answer.IsCorrect ? "Correto" : "Errou"
                                    : "Não iniciado";

                                return new ExercisesContentDTO
                                {
                                    Exercise = new ExerciseDTO
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
                                        IsFinalExercise = exercise.IsFinalExercise,
                                        ExercisePeriod = exercise.ExercisePeriod
                                    },
                                    Origin = flowItem.Origin,
                                    StateExercise = exerciseState,
                                    UserContainerExerciseFlowId = flowItem.Id
                                };
                            })
                            .Where(e => e != null)
                            .Cast<ExercisesContentDTO>()
                            .OrderBy(e => flowOrderById.GetValueOrDefault(e.UserContainerExerciseFlowId ?? -1))
                            .ToList();

                        if (!containerExercises.Any())
                            return null;

                        return new
                        {
                            FirstOrder = firstFlowItem.IndexOrder,
                            Blip = new BlipsDTO
                            {
                                ContainerExercise = new ContainerExerciseDTO
                                {
                                    Id = group.Min(task => task.Id),
                                    Title = group.Key.Name,
                                    ContainerDateTarget = group.Min(task => task.ContainerDateTargetInt),
                                    Exercises = containerExercises
                                },
                                StateContainer = containerState,
                                PhaseId = firstFlowItem.PhaseId
                            }
                        };
                    })
                    .Where(item => item != null)
                    .OrderBy(item => item!.FirstOrder)
                    .Select(item => item!.Blip)
                    .ToList();

                // Mapeados: exercícios de container + lowers filhos deles
                var mappedExerciseIds = flow
                    .Where(f => containerTaskExerciseIdSet.Contains(f.ExerciseId) ||
                                (f.Origin == FlowOrigin.Lower &&
                                 f.TriggeredByExerciseId.HasValue &&
                                 containerTaskExerciseIdSet.Contains(f.TriggeredByExerciseId.Value)))
                    .Select(f => f.ExerciseId)
                    .ToHashSet();

                var fallbackBlips = flow
                    .Where(flowItem => !mappedExerciseIds.Contains(flowItem.ExerciseId))
                    .OrderBy(flowItem => flowItem.IndexOrder)
                    .Select(flowItem =>
                    {
                        if (!exercisesById.TryGetValue(flowItem.ExerciseId, out var exercise))
                            return null;

                        var hasAnswer = answersByFlowId.TryGetValue(flowItem.Id, out var fallbackAnswer);
                        var fallbackExerciseState = hasAnswer
                            ? fallbackAnswer!.IsCorrect ? "Correto" : "Errou"
                            : "Não iniciado";
                        var fallbackContainerState = hasAnswer ? "Concluído" : "Não iniciado";

                        return new BlipsDTO
                        {
                            ContainerExercise = new ContainerExerciseDTO
                            {
                                Id = flowItem.Id,
                                Title = exercise.Title,
                                Exercises = new List<ExercisesContentDTO>
                                {
                                    new ExercisesContentDTO
                                    {
                                        Exercise = new ExerciseDTO
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
                                            IsFinalExercise = exercise.IsFinalExercise,
                                            ExercisePeriod = exercise.ExercisePeriod
                                        },
                                        Origin = flowItem.Origin,
                                        StateExercise = fallbackExerciseState,
                                        UserContainerExerciseFlowId = flowItem.Id
                                    }
                                }
                            },
                            StateContainer = fallbackContainerState,
                            PhaseId = flowItem.PhaseId
                        };
                    })
                    .Where(blip => blip != null)
                    .Cast<BlipsDTO>()
                    .ToList();

                blipsList.AddRange(fallbackBlips);
                blipsList = blipsList
                    .OrderBy(blip => blip.ContainerExercise.Exercises
                        .Min(e => flowOrderById.GetValueOrDefault(e.UserContainerExerciseFlowId ?? -1, int.MaxValue)))
                    .ToList();

                foreach (var blip in blipsList)
                {
                    var offset = blip.ContainerExercise.ContainerDateTarget;

                    if (offset == null)
                        continue;

                    var unlockDate = startDate.AddDays(offset.Value);

                    blip.UnlockDate = unlockDate;
                    blip.IsLocked = today < unlockDate;

                    if (blip.IsLocked)
                    {
                        blip.DaysRemaining = (unlockDate - today).Days;
                    }
                }

                var firstAvailableExercise = blipsList
                    .Where(b => !b.IsLocked)
                    .SelectMany(blip => blip.ContainerExercise.Exercises)
                    .Where(exercise => exercise.StateExercise == "Não iniciado")
                    .OrderBy(exercise => flowOrderById.GetValueOrDefault(exercise.UserContainerExerciseFlowId ?? -1, int.MaxValue))
                    .FirstOrDefault();

                if (firstAvailableExercise != null)
                {
                    firstAvailableExercise.StateExercise = "Atual";

                    var currentBlip = blipsList.FirstOrDefault(blip =>
                        blip.ContainerExercise.Exercises.Any(exercise =>
                            exercise.UserContainerExerciseFlowId == firstAvailableExercise.UserContainerExerciseFlowId));

                    if (currentBlip != null && currentBlip.StateContainer != "Concluído")
                        currentBlip.StateContainer = "Atual";
                }

                finalResult.Add(new IslandDTO
                {
                    Id = phase.Id,
                    Order = phase.Order,
                    Title = phase.Title,
                    Helper = phase.Helper,
                    Status = GetPhaseStatus(statusProgress),
                    Progress = statusProgress?.Progress ?? 0,
                    Blips = blipsList.ToArray()
                });
            }

            return CustomResponse<IEnumerable<IslandDTO>>
                .SuccessTrade(finalResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro ao buscar ilhas para o usuário {UserId} e fase {PhaseId}",
                userId, phaseId);

            return CustomResponse<IEnumerable<IslandDTO>>
                .Fail("Ocorreu um erro ao buscar as ilhas");
        }
    }

    private string GetPhaseStatus(ProgressStudentPhase? statusProgress)
    {
        return (int?)statusProgress?.Status switch
        {
            0 => "Não Iniciado",
            1 => "Em Progresso",
            2 => "Concluído",
            _ => "Desconhecido"
        };
    }

    public async Task CreateInitialFlowAsync(int userId, int phaseId)
    {
        var containers = await _exerciseRepository
            .GetContainerTasksByPhaseIdAsync(phaseId);

        var phaseExercises = await _exerciseRepository
            .GetExercisesByPhaseId(phaseId);

        var nonDailyContainers = containers
            .Where(c => !c.IsDailyTask)
            .ToList();

        int order = 0;
        var flows = new List<UserExerciseFlow>();
        var insertedExerciseIds = new HashSet<int>();

        var groupedContainers = nonDailyContainers
            .GroupBy(c => c.Name ?? string.Empty)
            .OrderBy(g => g.Min(x => x.ContainerDateTargetInt))
            .ThenBy(g => g.Min(x => x.Id));

        foreach (var containerGroup in groupedContainers)
        {
            var containerExercises = containerGroup
                .OrderBy(c => c.Exercise!.IndexOrder);

            foreach (var container in containerExercises)
            {
                var exercise = container.Exercise;

                if (exercise!.Difficulty != DifficultyLevel.Normal)
                    continue;

                if (!insertedExerciseIds.Add(exercise.Id))
                    continue;

                flows.Add(new UserExerciseFlow
                {
                    UserId = userId,
                    PhaseId = phaseId,
                    ExerciseId = exercise.Id,
                    IndexOrder = order++,
                    Origin = FlowOrigin.Main,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        var standaloneExercises = phaseExercises
            .Where(e => e.Difficulty != DifficultyLevel.Lower)
            .Where(e => !insertedExerciseIds.Contains(e.Id))
            .OrderBy(e => e.Difficulty == DifficultyLevel.ProofTest ? 1 : 0)
            .ThenBy(e => e.IndexOrder)
            .ThenBy(e => e.Id)
            .ToList();

        foreach (var exercise in standaloneExercises)
        {
            flows.Add(new UserExerciseFlow
            {
                UserId = userId,
                PhaseId = phaseId,
                ExerciseId = exercise.Id,
                IndexOrder = order++,
                Origin = FlowOrigin.Main,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!flows.Any())
            return;

        await _exerciseRepository.CreateUserExerciseFlowAsync(flows);
    }
}
