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
            var getModule = await _classRepository.GetModuleByPhaseIdAsync(phaseId);
            if (getModule == null)
            {
                return CustomResponse<IEnumerable<IslandDTO>>.Fail("Módulo não encontrado para a fase informada");
            }
            var phases = await _classRepository.GetPhasesByCurrentModuleAsync(getModule.Id);
            var finalResult = new List<IslandDTO>();

            foreach (var phase in phases)
            {
                var statusProgress = await _progressStudentPhaseRepository
                    .GetProgressByUserIdAndPhaseIdAsync(userId, phase.Id);

                var exercises = await _exerciseRepository
                    .GetExercisesByPhaseId(phase.Id);

                var answers = await _exerciseRepository
                    .GetExercisesAnswersForUserAsync(userId);

                var random = new Random();
                var blipsList = new List<BlipsDTO>();

                var mainExercises = exercises
                    .Where(e => e.Difficulty != DifficultyLevel.Lower && (e.IsDailyTask == false))
                    .OrderBy(e => e.IndexOrder)
                    .ToList();

                var lowerExercises = exercises
                    .Where(e => e.Difficulty == DifficultyLevel.Lower && (e.IsDailyTask == false))
                    .OrderBy(e => e.IndexOrder)
                    .ToList();

                foreach (var exercise in mainExercises)
                {
                    var answer = answers.FirstOrDefault(a => a.ExerciseId == exercise.Id || a.QuestionId == exercise.Id);

                    string state;

                    if (answer == null)
                        state = "Não iniciado";
                    else if (!answer.IsCorrect)
                        state = "Errou";
                    else
                        state = "Correto";

                    blipsList.Add(new BlipsDTO
                    {
                        Exercise = exercise,
                        State = state
                    });

                    if (answer != null && !answer.IsCorrect)
                    {
                        foreach (var lower in lowerExercises)
                        {
                            var lowerAnswer = answers.FirstOrDefault(a => a.ExerciseId == lower.Id || a.QuestionId == lower.Id);

                            string lowerState;

                            if (lowerAnswer == null)
                                lowerState = "Não iniciado";
                            else if (!lowerAnswer.IsCorrect)
                                lowerState = "Errou";
                            else
                                lowerState = "Correto";

                            if (!blipsList.Any(b => b.Exercise.Id == lower.Id))
                            {
                                blipsList.Add(new BlipsDTO
                                {
                                    Exercise = lower,
                                    State = lowerState
                                });
                            }
                        }
                    }
                }

                // Ajuste para marcar "Atual" apenas no primeiro "Não iniciado"
                bool currentAssigned = false;
                foreach (var blip in blipsList.OrderBy(b => b.Exercise.IndexOrder))
                {
                    if (!currentAssigned && blip.State == "Não iniciado")
                    {
                        blip.State = "Atual";
                        currentAssigned = true;
                    }
                }

                finalResult.Add(new IslandDTO
                {
                    Id = phase.Id,
                    Order = phase.Order,
                    Title = phase.Title,
                    Helper = phase.Helper,
                    Status = (int?)statusProgress?.Status == 0 ? "Não Iniciado" :
                             (int?)statusProgress?.Status == 1 ? "Em Progresso" :
                             (int?)statusProgress?.Status == 2 ? "Concluído" : "Desconhecido",
                    Progress = statusProgress?.Progress ?? 0,
                    Blips = blipsList.ToArray()
                });
            }

            if (finalResult == null || !finalResult.Any())
            {
                return CustomResponse<IEnumerable<IslandDTO>>.Fail("Nenhuma ilha encontrada para o usuário e fase especificados");
            }

            return CustomResponse<IEnumerable<IslandDTO>>.SuccessTrade(finalResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar ilhas para o usuário {UserId} e fase {PhaseId}", userId, phaseId);
            return CustomResponse<IEnumerable<IslandDTO>>.Fail("Ocorreu um erro ao buscar as ilhas");
        }
    }
}
