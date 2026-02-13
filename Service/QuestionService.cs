using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class QuestionService : IQuestionService
{
    private readonly ILogger<QuestionService> _logger;
    private readonly IQuestionRepository _questionRepository;

    public QuestionService(ILogger<QuestionService> logger, IQuestionRepository questionRepository)
    {
        _logger = logger;
        _questionRepository = questionRepository;
    }

    public async Task<CustomResponse<IEnumerable<Question>>> GetAllAsync()
    {
        var result = await _questionRepository.GetAllAsync();
        return CustomResponse<IEnumerable<Question>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<Question>> GetByIdAsync(int id)
    {
        var result = await _questionRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<Question>.Fail("Question not found")
            : CustomResponse<Question>.SuccessTrade(result);
    }
}
