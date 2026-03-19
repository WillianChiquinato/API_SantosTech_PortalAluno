using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class AnswerService : IAnswerService
{
    private readonly ILogger<AnswerService> _logger;
    private readonly IAnswerRepository _answerRepository;

    public AnswerService(ILogger<AnswerService> logger, IAnswerRepository answerRepository)
    {
        _logger = logger;
        _answerRepository = answerRepository;
    }

    public async Task<CustomResponse<IEnumerable<Answer>>> GetAllAsync()
    {
        var result = await _answerRepository.GetAllAsync();
        return CustomResponse<IEnumerable<Answer>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<Answer>> GetByIdAsync(int id)
    {
        try
        {
            var result = await _answerRepository.GetByIdAsync(id);
            return result == null
                ? CustomResponse<Answer>.Fail("Resposta não encontrada")
                : CustomResponse<Answer>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar resposta por ID.");
            return CustomResponse<Answer>.Fail("Ocorreu um erro ao buscar resposta por ID.");
        }
    }

    public async Task<CustomResponse<AnswerAndUnreadResponsesCountDTO>> GetByUserIdAsync(int userId)
    {
        try
        {
            var answers = await _answerRepository.GetByUserIdAsync(userId);
            var newAnswersCount = await _answerRepository.GetNewAnswersByUserIdCount(userId);

            var exercisesByAnswers = await _answerRepository.GetExercisesByAnswerIdsAsync(answers.Select(a => a.Id).ToList());
            var selectedOptionTexts = await _answerRepository.GetSelectedOptionTextsByAnswerIdsAsync(answers.Select(a => a.Id).ToList());

            var answerDTOs = answers.Select(a => new AnswerDTO
            {
                Id = a.Id,
                UserId = a.UserId,
                QuestionId = a.QuestionId,
                Exercise = exercisesByAnswers.ContainsKey(a.Id) ? new ExerciseDTO
                {
                    Id = exercisesByAnswers[a.Id].Id,
                    Title = exercisesByAnswers[a.Id].Title,
                    Description = exercisesByAnswers[a.Id].Description,
                    PointsRedeem = exercisesByAnswers[a.Id].PointsRedeem,
                    Difficulty = exercisesByAnswers[a.Id].Difficulty
                } : null,
                UserExerciseFlowId = a.UserExerciseFlowId,
                AnswerText = a.AnswerText,
                SelectedOption = selectedOptionTexts.ContainsKey(a.Id) ? selectedOptionTexts[a.Id] : null,
                IsCorrect = a.IsCorrect,
                AnsweredAt = a.AnsweredAt,
                Feedback = a.Feedback
            }).ToList();

            var exerciseGroups = answerDTOs
                .GroupBy(a => a.Exercise?.Id)
                .Select(g => new AnswerGroupByExerciseDTO
                {
                    Exercise = g.First().Exercise,
                    Answers = g.OrderByDescending(a => a.AnsweredAt).ToList()
                })
                .ToList();

            var result = new AnswerAndUnreadResponsesCountDTO
            {
                ExerciseGroups = exerciseGroups,
                UnreadResponsesCount = newAnswersCount
            };

            return CustomResponse<AnswerAndUnreadResponsesCountDTO>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar respostas por User ID.");
            return CustomResponse<AnswerAndUnreadResponsesCountDTO>.Fail("Ocorreu um erro ao buscar respostas por User ID.");
        }
    }
}