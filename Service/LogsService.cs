using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class LogsService
{
    private readonly ILogger<LogsService> _logger;
    private readonly ILogsRepository _logsRepository;

    public LogsService(ILogger<LogsService> logger, ILogsRepository logsRepository)
    {
        _logger = logger;
        _logsRepository = logsRepository;
    }

    public async Task<CustomResponse<IEnumerable<Logs>>> GetLogsAsync()
    {
        try
        {
            var result = await _logsRepository.GetLogsAsync();

            return CustomResponse<IEnumerable<Logs>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os logs");
            return CustomResponse<IEnumerable<Logs>>.Fail("Ocorreu um erro ao buscar todos os logs");
        }
    }

    public async Task<CustomResponse<IEnumerable<Logs>>> GetLogsByUserIdAsync(int userId)
    {
        try
        {
            var result = await _logsRepository.GetLogsByUserIdAsync(userId);
            return result == null || !result.Any()
                ? CustomResponse<IEnumerable<Logs>>.Fail("Log nao encontrado para o User ID: " + userId)
                : CustomResponse<IEnumerable<Logs>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar log para o User ID: {UserId}", userId);
            return CustomResponse<IEnumerable<Logs>>.Fail("Ocorreu um erro ao buscar o log para o User ID: " + userId);
        }
    }
}
