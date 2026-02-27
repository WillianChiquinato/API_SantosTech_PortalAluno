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

            foreach (var phase in phases)
            {
                var statusProgress = await _progressStudentPhaseRepository
                    .GetProgressByUserIdAndPhaseIdAsync(userId, phase.Id);

                var flow = await _exerciseRepository
                        .GetByUserAndPhaseOrderedAsync(userId, phase.Id);

                if (!flow.Any())
                {
                    await CreateInitialFlowAsync(userId, phase.Id);

                    flow = await _exerciseRepository
                        .GetByUserAndPhaseOrderedAsync(userId, phase.Id);
                }

                var flowExerciseIds = flow
                    .Select(f => f.ExerciseId)
                    .ToList();

                var flowExercises = await _exerciseRepository
                    .GetFlowWithExercisesAsync(userId, phase.Id);

                var exercisesById = flowExercises
                    .DistinctBy(e => e.Id)
                    .ToDictionary(e => e.Id);

                var flowIds = flow.Select(f => f.Id).ToList();
                var phaseAnswers = userAnswers
                    .Where(a => flowIds.Contains(a.UserExerciseFlowId))
                    .ToList();

                var latestAnswers = phaseAnswers
                    .GroupBy(a => a.UserExerciseFlowId)
                    .Select(g => g.OrderByDescending(x => x.SubmittedAt).First())
                    .ToList();

                var answersByExercise = latestAnswers
                    .ToDictionary(a => a.UserExerciseFlowId);

                var blipsList = new List<BlipsDTO>();

                foreach (var flowItem in flow.OrderBy(f => f.IndexOrder))
                {
                    if (!exercisesById.TryGetValue(flowItem.ExerciseId, out var exercise))
                        continue;

                    string state;

                    if (!answersByExercise.TryGetValue(flowItem.Id, out var answer))
                    {
                        state = "Não iniciado";
                    }
                    else
                    {
                        state = answer.IsCorrect ? "Correto" : "Errou";
                    }

                    var exerciseDto = new ExerciseDTO
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
                    };

                    blipsList.Add(new BlipsDTO
                    {
                        Exercise = exerciseDto,
                        State = state,
                        Origin = flowItem.Origin
                    });
                }

                var firstNotStarted = blipsList
                    .FirstOrDefault(b => b.State == "Não iniciado");

                if (firstNotStarted != null)
                    firstNotStarted.State = "Atual";

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
        var exercises = await _exerciseRepository
            .GetExercisesByPhaseId(phaseId);

        var mainExercises = exercises
            .Where(e => e.Difficulty == DifficultyLevel.Normal)
            .OrderBy(e => e.IndexOrder)
            .ToList();

        if (!mainExercises.Any())
            return;

        int order = 0;

        var initialFlow = mainExercises
            .Select(e => new UserExerciseFlow
            {
                UserId = userId,
                PhaseId = phaseId,
                ExerciseId = e.Id,
                IndexOrder = order++,
                Origin = FlowOrigin.Main,
                TriggeredByExerciseId = null,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        await _exerciseRepository.CreateUserExerciseFlowAsync(initialFlow);
    }
}
