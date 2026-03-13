using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class ExerciseService : IExerciseService
{
    private readonly ILogger<ExerciseService> _logger;
    private readonly IExerciseRepository _exerciseRepository;

    public ExerciseService(ILogger<ExerciseService> logger, IExerciseRepository exerciseRepository)
    {
        _logger = logger;
        _exerciseRepository = exerciseRepository;
    }

    public async Task<CustomResponse<IEnumerable<Exercise>>> GetAllAsync()
    {
        try
        {
            var result = await _exerciseRepository.GetAllAsync();
            return CustomResponse<IEnumerable<Exercise>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os exercícios");
            return CustomResponse<IEnumerable<Exercise>>.Fail("Ocorreu um erro ao buscar os exercícios");
        }
    }

    public async Task<CustomResponse<Exercise>> GetByIdAsync(int id)
    {
        try
        {
            var result = await _exerciseRepository.GetByIdAsync(id);
            return result == null ? CustomResponse<Exercise>.Fail("Exercício não encontrado") : CustomResponse<Exercise>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar exercício por ID {ExerciseId}", id);
            return CustomResponse<Exercise>.Fail("Ocorreu um erro ao buscar o exercício");
        }
    }

    public async Task<CustomResponse<IEnumerable<ExerciseDailyTasksDTO>>> GetDailyTasksForPhaseAsync(int phaseId, int userId)
    {
        try
        {
            var result = await _exerciseRepository.GetDailyTasksForPhaseAsync(phaseId, userId);
            var exercisesAnswers = await _exerciseRepository.GetDailyExercisesAnswersForPhaseAsync(phaseId, userId);

            result.ForEach(task =>
            {
                task.Exercises.ForEach(exercise =>
                {
                    exercise.IsCompletedAnswer = exercisesAnswers.Any(answer => answer.ExerciseId == exercise.Id);
                });
            });

            return result == null ? CustomResponse<IEnumerable<ExerciseDailyTasksDTO>>.Fail("Nenhum exercício diário encontrado para a fase especificada") : CustomResponse<IEnumerable<ExerciseDailyTasksDTO>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar exercícios diários para a fase {PhaseId} e usuário {UserId}", phaseId, userId);
            return CustomResponse<IEnumerable<ExerciseDailyTasksDTO>>.Fail("Ocorreu um erro ao buscar os exercícios diários para a fase especificada");
        }
    }

    public async Task<CustomResponse<IEnumerable<QuestionOptionsDTO>>> GetQuestionsOptionsForExerciseAsync(int exerciseId)
    {
        try
        {
            var result = await _exerciseRepository.GetQuestionsOptionsForExerciseAsync(exerciseId);
            return result == null ? CustomResponse<IEnumerable<QuestionOptionsDTO>>.Fail("Nenhuma opção de pergunta encontrada para o exercício especificado") : CustomResponse<IEnumerable<QuestionOptionsDTO>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar opções de perguntas para o exercício {ExerciseId}", exerciseId);
            return CustomResponse<IEnumerable<QuestionOptionsDTO>>.Fail("Ocorreu um erro ao buscar as opções de perguntas para o exercício especificado");
        }
    }

    public async Task<CustomResponse<bool>> SubmitExerciseAnswersAsync(ExerciseSubmissionDTO submission)
    {
        try
        {
            var result = await _exerciseRepository.SubmitExerciseAnswersAsync(submission);

            var exercise = await _exerciseRepository.GetByIdAsync(submission.ExerciseId);
            if (exercise == null)
            {
                return CustomResponse<bool>.Fail("Exercício não encontrado");
            }

            var effectivePhaseId = submission.PhaseId.GetValueOrDefault();
            if (effectivePhaseId == 0)
            {
                effectivePhaseId = exercise.PhaseId;
            }

            if (!exercise.IsDailyTask && !submission.SubmissionData!.IsCorrect)
            {
                await InsertLowerExercisesAsync(
                    submission.UserId,
                    effectivePhaseId,
                    exercise.Id,
                    submission.UserExerciseFlowId);
            }

            return result ? CustomResponse<bool>.SuccessTrade(true) : CustomResponse<bool>.Fail("Falha ao processar as respostas do exercício");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar as respostas do exercício");
            return CustomResponse<bool>.Fail("Ocorreu um erro ao processar as respostas do exercício");
        }
    }

    public async Task InsertLowerExercisesAsync(int userId, int phaseId, int exerciseId, int? userExerciseFlowId)
    {
        try
        {
            await _exerciseRepository.InsertLowerExercisesAsync(userId, phaseId, exerciseId, userExerciseFlowId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inserir exercícios inferiores para o usuário {UserId}, fase {PhaseId} e exercício {ExerciseId}", userId, phaseId, exerciseId);
        }
    }

    public async Task<CustomResponse<int>> SyncMainExercisesIntoPhaseFlowsAsync(int phaseId)
    {
        try
        {
            var insertedCount = await _exerciseRepository.SyncMainExercisesIntoExistingFlowsAsync(phaseId);
            return CustomResponse<int>.SuccessTrade(insertedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao sincronizar exercícios MAIN para a fase {PhaseId}", phaseId);
            return CustomResponse<int>.Fail("Ocorreu um erro ao sincronizar o fluxo MAIN da fase");
        }
    }

    public async Task<CustomResponse<VerifyDTO>> VerifyExistingAnswersAsync(int exerciseId, int userId)
    {
        try
        {
            var result = await _exerciseRepository.VerifyExistingAnswersAsync(exerciseId, userId);
            return CustomResponse<VerifyDTO>.SuccessTrade(new VerifyDTO { ExistingAnswers = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar respostas existentes para o exercício {ExerciseId} e usuário {UserId}", exerciseId, userId);
            return CustomResponse<VerifyDTO>.Fail("Ocorreu um erro ao verificar respostas existentes para o exercício especificado");
        }
    }
}
