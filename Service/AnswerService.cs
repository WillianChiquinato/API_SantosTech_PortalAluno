using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
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
        var result = await _answerRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<Answer>.Fail("Answer not found")
            : CustomResponse<Answer>.SuccessTrade(result);
    }
}