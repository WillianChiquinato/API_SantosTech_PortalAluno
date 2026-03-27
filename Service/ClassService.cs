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
    private readonly IModuleRepository _moduleRepository;
    private readonly IProgressStudentPhaseRepository _progressStudentPhaseRepository;
    private readonly IExerciseRepository _exerciseRepository;

    public ClassService(ILogger<ClassService> logger, IClassRepository classRepository, IModuleRepository moduleRepository, IProgressStudentPhaseRepository progressStudentPhaseRepository, IExerciseRepository exerciseRepository)
    {
        _logger = logger;
        _classRepository = classRepository;
        _moduleRepository = moduleRepository;
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

    public async Task<CustomResponse<IEnumerable<IslandDTO>>> GetIslandsByUserIdAndCurrentModuleAsync(int userId, int moduleId)
    {
        try
        {
            var module = await _moduleRepository.GetByIdAsync(moduleId);
            if (module == null)
                return CustomResponse<IEnumerable<IslandDTO>>.Fail("Módulo não encontrado");

            var phases = await _classRepository.GetPhasesByCurrentModuleAsync(moduleId);
            if (phases == null || !phases.Any())
                return CustomResponse<IEnumerable<IslandDTO>>.Fail("Nenhuma ilha encontrada");

            var userAnswers = await _exerciseRepository.GetExercisesAnswersForUserAsync(userId);
            var userCourse = await _classRepository.GetByCourseAndModuleIdAsync(module.CourseId, moduleId);
            if (userCourse == null)
                return CustomResponse<IEnumerable<IslandDTO>>.Fail("Curso do módulo não encontrado");

            var startDate = userCourse!.StartDate.Date;
            var today = DateTime.UtcNow.Date;

            var islands = new List<IslandDTO>();

            foreach (var phase in phases)
            {
                await EnsureUserFlow(userId, phase.Id);

                var flow = await _exerciseRepository.GetByUserAndPhaseOrderedAsync(userId, phase.Id);
                var exercises = await _exerciseRepository.GetFlowWithExercisesAsync(userId, phase.Id);
                var containers = await _exerciseRepository.GetNonDailyContainerTasksByPhaseAsync(phase.Id);

                var exercisesById = exercises.DistinctBy(e => e.Id).ToDictionary(e => e.Id);

                var answersByFlow = GetLatestAnswers(userAnswers, flow);
                var lowerFlowsByTrigger = BuildTriggeredLowerFlowMap(flow);

                var containerBlips = BuildContainerBlips(flow, containers, exercisesById, answersByFlow, lowerFlowsByTrigger);

                var lastContainerDay = containers.Any()
                    ? containers.Max(c => c.ContainerDateTargetInt)
                    : 0;
                var proofBlip = BuildProofBlip(flow, exercisesById, answersByFlow, lowerFlowsByTrigger, lastContainerDay ?? 0);

                if (proofBlip != null)
                    containerBlips.Add(proofBlip);

                containerBlips = containerBlips
                    .OrderBy(b => b.ContainerExercise.ContainerDateTarget ?? int.MaxValue)
                    .ToList();

                ApplyUnlockRules(containerBlips, startDate, today);

                ResolveCurrentExercise(containerBlips, flow);

                var progress = await _progressStudentPhaseRepository
                    .GetProgressByUserIdAndPhaseIdAsync(userId, phase.Id);

                islands.Add(new IslandDTO
                {
                    Id = phase.Id,
                    Order = phase.Order,
                    Title = phase.Title,
                    Helper = phase.Helper,
                    Status = GetPhaseStatus(progress),
                    Progress = progress?.Progress ?? 0,
                    Blips = containerBlips.ToArray()
                });
            }

            return CustomResponse<IEnumerable<IslandDTO>>.SuccessTrade(islands);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar ilhas");
            return CustomResponse<IEnumerable<IslandDTO>>.Fail("Erro ao buscar ilhas");
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

    private async Task EnsureUserFlow(int userId, int phaseId)
    {
        await _exerciseRepository.SyncMainExercisesForUserPhaseAsync(userId, phaseId);

        var flow = await _exerciseRepository.GetByUserAndPhaseOrderedAsync(userId, phaseId);

        if (!flow.Any())
            await CreateInitialFlowAsync(userId, phaseId);
    }

    private Dictionary<int, ExerciseAnswerDTO> GetLatestAnswers(
    IEnumerable<ExerciseAnswerDTO> answers,
    List<UserExerciseFlow> flow)
    {
        var flowIds = flow.Select(f => f.Id).ToHashSet();

        return answers
            .Where(a => flowIds.Contains(a.UserExerciseFlowId))
            .GroupBy(a => a.UserExerciseFlowId)
            .Select(g => g.OrderByDescending(x => x.SubmittedAt).First())
            .ToDictionary(a => a.UserExerciseFlowId);
    }

    private Dictionary<int, List<UserExerciseFlow>> BuildTriggeredLowerFlowMap(List<UserExerciseFlow> flow)
    {
        return flow
            .Where(f => f.Origin == FlowOrigin.Lower && f.TriggeredByExerciseId.HasValue)
            .GroupBy(f => f.TriggeredByExerciseId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(f => f.IndexOrder).ToList());
    }

    private List<BlipsDTO> BuildContainerBlips(
    List<UserExerciseFlow> flow,
    List<ContainerTask> containers,
    Dictionary<int, Exercise> exercisesById,
    Dictionary<int, ExerciseAnswerDTO> answers,
    Dictionary<int, List<UserExerciseFlow>> lowerFlowsByTrigger)
    {
        var mainFlowByExercise = flow
            .Where(f => f.Origin == FlowOrigin.Main)
            .GroupBy(f => f.ExerciseId)
            .ToDictionary(g => g.Key, g => g.OrderBy(f => f.IndexOrder).First());

        return containers
            .GroupBy(c => c.Name)
            .Select(group =>
            {
                var containerExerciseIds = group
                    .Select(c => c.ExerciseId)
                    .ToHashSet();

                var containerMainFlows = group
                    .Select(c => mainFlowByExercise.GetValueOrDefault(c.ExerciseId))
                    .Where(f => f is not null)
                    .Cast<UserExerciseFlow>()
                    .OrderBy(f => f.IndexOrder)
                    .ToList();

                if (!containerMainFlows.Any())
                    return null;

                var orderedFlows = new List<UserExerciseFlow>();

                foreach (var mainFlow in containerMainFlows)
                {
                    orderedFlows.Add(mainFlow);

                    if (!lowerFlowsByTrigger.TryGetValue(mainFlow.ExerciseId, out var lowers))
                        continue;

                    orderedFlows.AddRange(
                        lowers
                            .Where(l => l.TriggeredByExerciseId == mainFlow.ExerciseId)
                            .OrderBy(l => l.IndexOrder));
                }

                var exercises = orderedFlows
                    .Select(flowItem =>
                    {
                        if (!exercisesById.TryGetValue(flowItem.ExerciseId, out var exercise))
                            return null;

                        var state = answers.TryGetValue(flowItem.Id, out var ans)
                            ? ans.IsCorrect ? "Correto" : "Errou"
                            : "Não iniciado";

                        return new ExercisesContentDTO
                        {
                            Exercise = new ExerciseDTO
                            {
                                Id = exercise.Id,
                                Title = exercise.Title,
                                Description = exercise.Description,
                                Difficulty = exercise.Difficulty,
                                IndexOrder = exercise.IndexOrder,
                                PointsRedeem = exercise.PointsRedeem
                            },
                            Origin = flowItem.Origin,
                            StateExercise = state,
                            UserContainerExerciseFlowId = flowItem.Id
                        };
                    })
                    .Where(e => e != null)
                    .Cast<ExercisesContentDTO>()
                    .OrderBy(e => e.Exercise.IndexOrder)
                    .ToList();

                return new BlipsDTO
                {
                    ContainerExercise = new ContainerExerciseDTO
                    {
                        Id = group.Min(c => c.Id),
                        Title = group.Key,
                        ContainerDateTarget = group.Min(c => c.ContainerDateTargetInt),
                        Exercises = exercises
                    },
                    StateContainer = exercises.All(e => e.StateExercise != "Não iniciado")
                        ? "Concluído"
                        : "Não iniciado",
                    PhaseId = containerMainFlows.First().PhaseId
                };
            })
            .Where(b => b != null)
            .Cast<BlipsDTO>()
            .OrderBy(b => b.ContainerExercise.ContainerDateTarget ?? int.MaxValue)
            .ToList();
    }

    private BlipsDTO? BuildProofBlip(
    List<UserExerciseFlow> flow,
    Dictionary<int, Exercise> exercisesById,
    Dictionary<int, ExerciseAnswerDTO> answers,
    Dictionary<int, List<UserExerciseFlow>> lowerFlowsByTrigger,
    int lastContainerDay)
    {
        var proofExercise = exercisesById
            .Values
            .FirstOrDefault(e => e.Difficulty == DifficultyLevel.ProofTest);

        if (proofExercise == null)
            return null;

        var proofFlow = flow.FirstOrDefault(f => f.ExerciseId == proofExercise.Id);

        if (proofFlow == null)
            return null;

        var orderedFlows = ExpandFlowsWithTriggeredLowers(new List<UserExerciseFlow> { proofFlow }, lowerFlowsByTrigger);
        var exercises = BuildExercisesContent(orderedFlows, exercisesById, answers);

        return new BlipsDTO
        {
            ContainerExercise = new ContainerExerciseDTO
            {
                Id = proofFlow.Id,
                Title = proofExercise.Title,
                ContainerDateTarget = lastContainerDay + 1,
                Exercises = exercises
            },
            StateContainer = exercises.All(e => e.StateExercise != "Não iniciado")
                ? "Concluído"
                : "Não iniciado",
            PhaseId = proofFlow.PhaseId
        };
    }

    private List<UserExerciseFlow> ExpandFlowsWithTriggeredLowers(
    List<UserExerciseFlow> mainFlows,
    Dictionary<int, List<UserExerciseFlow>> lowerFlowsByTrigger)
    {
        var orderedFlows = new List<UserExerciseFlow>();

        foreach (var mainFlow in mainFlows.OrderBy(f => f.IndexOrder))
        {
            orderedFlows.Add(mainFlow);

            if (!lowerFlowsByTrigger.TryGetValue(mainFlow.ExerciseId, out var lowerFlows))
                continue;

            orderedFlows.AddRange(lowerFlows);
        }

        return orderedFlows;
    }

    private List<ExercisesContentDTO> BuildExercisesContent(
    List<UserExerciseFlow> orderedFlows,
    Dictionary<int, Exercise> exercisesById,
    Dictionary<int, ExerciseAnswerDTO> answers)
    {
        return orderedFlows
            .Select(flowItem =>
            {
                if (!exercisesById.TryGetValue(flowItem.ExerciseId, out var exercise))
                    return null;

                return new ExercisesContentDTO
                {
                    Exercise = new ExerciseDTO
                    {
                        Id = exercise.Id,
                        Title = exercise.Title,
                        Description = exercise.Description,
                        Difficulty = exercise.Difficulty,
                        IndexOrder = exercise.IndexOrder
                    },
                    Origin = flowItem.Origin,
                    StateExercise = GetExerciseState(flowItem.Id, answers),
                    UserContainerExerciseFlowId = flowItem.Id
                };
            })
            .Where(exercise => exercise is not null)
            .Cast<ExercisesContentDTO>()
            .ToList();
    }

    private string GetExerciseState(int userExerciseFlowId, Dictionary<int, ExerciseAnswerDTO> answers)
    {
        if (!answers.TryGetValue(userExerciseFlowId, out var answer))
            return "Não iniciado";

        return answer.IsCorrect ? "Correto" : "Errou";
    }

    private void ApplyUnlockRules(List<BlipsDTO> blips, DateTime startDate, DateTime today)
    {
        BlipsDTO? previous = null;

        foreach (var blip in blips)
        {
            var offset = blip.ContainerExercise.ContainerDateTarget;

            if (offset == null)
            {
                blip.IsLocked = false;
                previous = blip;
                continue;
            }

            var unlockDate = startDate.AddDays(offset.Value);

            blip.UnlockDate = unlockDate;

            var dateUnlocked = today >= unlockDate;
            var previousCompleted = previous == null || previous.StateContainer == "Concluído";

            blip.IsLocked = !(dateUnlocked && previousCompleted);

            if (blip.IsLocked)
                blip.DaysRemaining = Math.Max((unlockDate - today).Days, 0);

            previous = blip;
        }
    }

    private void ResolveCurrentExercise(List<BlipsDTO> blips, List<UserExerciseFlow> flow)
    {
        var orderMap = flow.ToDictionary(f => f.Id, f => f.IndexOrder);

        var nextExercise = blips
            .Where(b => !b.IsLocked)
            .SelectMany(b => b.ContainerExercise.Exercises)
            .Where(e => e.StateExercise == "Não iniciado")
            .OrderBy(e => orderMap.GetValueOrDefault(e.UserContainerExerciseFlowId ?? -1))
            .FirstOrDefault();

        if (nextExercise == null)
            return;

        nextExercise.StateExercise = "Atual";

        var blip = blips.First(b =>
            b.ContainerExercise.Exercises.Any(e =>
                e.UserContainerExerciseFlowId == nextExercise.UserContainerExerciseFlowId));

        if (!blip.IsLocked && blip.StateContainer != "Concluído")
            blip.StateContainer = "Atual";
    }

    public async Task<CustomResponse<IEnumerable<ClassRoomDTO>>> GetClassRoomsByClassIdAsync(int classId)
    {
        try
        {
            var classRooms = await _classRepository.GetClassRoomsByClassIdAsync(classId);
            if (classRooms == null || !classRooms.Any())
                return CustomResponse<IEnumerable<ClassRoomDTO>>.Fail("Nenhuma sala de aula encontrada para esta classe");

            var className = await _classRepository.GetByIdAsync(classId);
            var getExercisesByClassRoomTasks = await _exerciseRepository.GetExercisesByClassRoomIdsAsync(classRooms.Select(cr => cr.Id).ToList());

            var classRoomDTOs = classRooms.Select(cr => new ClassRoomDTO
            {
                Id = cr.Id,
                ClassName = className?.Name!,
                Name = cr.Name,
                TargetLimited = cr.TargetLimited ?? DateTime.Now.AddHours(1),
                Exercises = getExercisesByClassRoomTasks
                    .Select(e => new ExerciseDTO
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Description = e.Description,
                        VideoUrl = e.VideoUrl,
                        Difficulty = e.Difficulty,
                        IndexOrder = e.IndexOrder,
                        PointsRedeem = e.PointsRedeem
                    })
                    .ToList(),
                CreatedAt = cr.CreatedAt
            }).ToList();

            return CustomResponse<IEnumerable<ClassRoomDTO>>.SuccessTrade(classRoomDTOs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar salas de aula para a classe com ID {ClassId}", classId);
            return CustomResponse<IEnumerable<ClassRoomDTO>>.Fail("Ocorreu um erro ao buscar as salas de aula");
        }
    }
}
