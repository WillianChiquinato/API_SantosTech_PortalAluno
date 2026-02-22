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

    public async Task<CustomResponse<IEnumerable<ExerciseDailyTasksDTO>>> GetDailyTasksForPhaseAsync(int phaseId)
    {
        try
        {
            var result = await _exerciseRepository.GetDailyTasksForPhaseAsync(phaseId);
            return result == null ? CustomResponse<IEnumerable<ExerciseDailyTasksDTO>>.Fail("Nenhum exercício diário encontrado para a fase especificada") : CustomResponse<IEnumerable<ExerciseDailyTasksDTO>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar exercícios diários para a fase {PhaseId}", phaseId);
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
            return result ? CustomResponse<bool>.SuccessTrade(true) : CustomResponse<bool>.Fail("Falha ao processar as respostas do exercício");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar as respostas do exercício");
            return CustomResponse<bool>.Fail("Ocorreu um erro ao processar as respostas do exercício");
        }
    }
}
