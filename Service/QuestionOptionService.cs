using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class QuestionOptionService : IQuestionOptionService
{
    private readonly ILogger<QuestionOptionService> _logger;
    private readonly IQuestionOptionRepository _questionOptionRepository;

    public QuestionOptionService(
        ILogger<QuestionOptionService> logger,
        IQuestionOptionRepository questionOptionRepository)
    {
        _logger = logger;
        _questionOptionRepository = questionOptionRepository;
    }

    public async Task<CustomResponse<IEnumerable<QuestionOption>>> GetAllAsync()
    {
        var result = await _questionOptionRepository.GetAllAsync();
        return CustomResponse<IEnumerable<QuestionOption>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<QuestionOption>> GetByIdAsync(int id)
    {
        var result = await _questionOptionRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<QuestionOption>.Fail("Question option not found")
            : CustomResponse<QuestionOption>.SuccessTrade(result);
    }
}
