using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Jobs;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;
using Hangfire;

namespace API_PortalSantosTech.Services;

public class PointService : IPointService
{
    private readonly ILogger<PointService> _logger;
    private readonly IPointRepository _pointRepository;

    public PointService(ILogger<PointService> logger, IPointRepository pointRepository)
    {
        _logger = logger;
        _pointRepository = pointRepository;
    }

    public async Task<CustomResponse<IEnumerable<Point>>> GetAllAsync()
    {
        try
        {
            var result = await _pointRepository.GetAllAsync();
            return CustomResponse<IEnumerable<Point>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pontos");
            return CustomResponse<IEnumerable<Point>>.Fail("Erro ao buscar pontos");
        }
    }

    public async Task<CustomResponse<Point>> GetByIdAsync(int id)
    {
        try
        {
            var result = await _pointRepository.GetByIdAsync(id);
            return result == null
                ? CustomResponse<Point>.Fail("Point not found")
                : CustomResponse<Point>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar ponto por ID");
            return CustomResponse<Point>.Fail("Erro ao buscar ponto por ID");
        }
    }

    public async Task<CustomResponse<IEnumerable<PointRankingDTO>>> GetRankingAsync()
    {
        try
        {
            var result = await _pointRepository.GetRankingAsync();
            return CustomResponse<IEnumerable<PointRankingDTO>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar ranking de pontos");
            return CustomResponse<IEnumerable<PointRankingDTO>>.Fail("Erro ao buscar ranking de pontos");
        }
    }

    public async Task<CustomResponse<ExercisePointAwardResult>> AddPointsForUserAsync(AddPointsDTO addPoints)
    {
        try
        {
            var awardResult = await _pointRepository.AddPointsForUserAsync(addPoints.UserId, addPoints.ExerciseId);

            if (!awardResult.Success)
                return CustomResponse<ExercisePointAwardResult>.Fail("Nao foi possivel contabilizar os pontos do exercicio");

            if (awardResult.AlreadyAwarded)
            {
                awardResult.Message = "Os pontos deste exercicio ja foram contabilizados.";
                return CustomResponse<ExercisePointAwardResult>.SuccessTrade(awardResult);
            }

            awardResult.Message = awardResult.PointsAwarded > 0
                ? $"Voce ganhou {awardResult.PointsAwarded} ponto(s) neste exercicio."
                : "Exercicio enviado sem pontuacao nesta tentativa.";

            return CustomResponse<ExercisePointAwardResult>.SuccessTrade(awardResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar pontos para o usuario");
            return CustomResponse<ExercisePointAwardResult>.Fail("Erro ao adicionar pontos para o usuario");
        }
    }

    public async Task<CustomResponse<IEnumerable<RankingCategoryDTO>>> GetRankingCategoriesAsync()
    {
        try
        {
            var result = await _pointRepository.GetRankingCategoriesAsync();
            return CustomResponse<IEnumerable<RankingCategoryDTO>>.SuccessTrade(result, result.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar categorias disponíveis para ranking");
            return CustomResponse<IEnumerable<RankingCategoryDTO>>.Fail("Erro ao buscar categorias disponíveis para ranking");
        }
    }

    public async Task<CustomResponse<IEnumerable<RankingPerCategoryDTO>>> GetAvailableRankingPerCategoryAsync(string? category, int? limit, int offset)
    {
        try
        {
            int? normalizedLimit = limit.HasValue ? Math.Clamp(limit.Value, 1, 100) : null;
            var normalizedOffset = Math.Max(offset, 0);
            var (items, totalRows) = await _pointRepository.GetAvailableRankingPerCategoryAsync(
                category,
                normalizedLimit,
                normalizedOffset);
            return CustomResponse<IEnumerable<RankingPerCategoryDTO>>.SuccessTrade(items, totalRows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar ranking disponível por categoria");
            return CustomResponse<IEnumerable<RankingPerCategoryDTO>>.Fail("Erro ao buscar ranking disponível por categoria");
        }
    }

    public async Task<CustomResponse<IEnumerable<EventRankingDTO>>> GetRankingEventAsync(int eventType)
    {
        try
        {
            var result = await _pointRepository.GetRankingEventAsync(eventType);
            return CustomResponse<IEnumerable<EventRankingDTO>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar ranking de eventos");
            return CustomResponse<IEnumerable<EventRankingDTO>>.Fail("Erro ao buscar ranking de eventos");
        }
    }

    public async Task<CustomResponse<IEnumerable<RankingEventHistoryDTO>>> GetRankingEventHistoryAsync(int? eventType, int limit, int offset)
    {
        try
        {
            if (eventType.HasValue && !Enum.IsDefined(typeof(EventType), eventType.Value))
                return CustomResponse<IEnumerable<RankingEventHistoryDTO>>.Fail("Tipo do evento inválido.");

            var normalizedLimit = Math.Clamp(limit, 1, 100);
            var normalizedOffset = Math.Max(offset, 0);
            var (items, totalRows) = await _pointRepository.GetRankingEventHistoryAsync(
                eventType,
                normalizedLimit,
                normalizedOffset);
            return CustomResponse<IEnumerable<RankingEventHistoryDTO>>.SuccessTrade(items, totalRows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar histórico de ranking de eventos");
            return CustomResponse<IEnumerable<RankingEventHistoryDTO>>.Fail("Erro ao buscar histórico de ranking de eventos");
        }
    }

    public async Task<CustomResponse<EventRankingDTO>> GetRankingEventByIdAsync(int id)
    {
        try
        {
            var result = await _pointRepository.GetRankingEventByIdAsync(id);
            return result == null
                ? CustomResponse<EventRankingDTO>.Fail("Evento de ranking não encontrado.")
                : CustomResponse<EventRankingDTO>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar evento de ranking por ID");
            return CustomResponse<EventRankingDTO>.Fail("Erro ao buscar evento de ranking por ID");
        }
    }

    public async Task<CustomResponse<RankingEventDTO>> CreateRankingEventAsync(RankingEventUpsertDTO request)
    {
        try
        {
            var validationError = ValidateRankingEvent(request);
            if (validationError != null)
                return CustomResponse<RankingEventDTO>.Fail(validationError);

            var result = await _pointRepository.CreateRankingEventAsync(request);
            var scheduleResponse = await ScheduleRankingEventAsync(result.Id);

            if (!scheduleResponse.Success)
                _logger.LogWarning("Evento {EventId} criado, mas não foi possível agendar o job.", result.Id);

            return CustomResponse<RankingEventDTO>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar evento de ranking");
            return CustomResponse<RankingEventDTO>.Fail("Erro ao criar evento de ranking");
        }
    }

    public async Task<CustomResponse<RankingEventDTO>> UpdateRankingEventAsync(int id, RankingEventUpsertDTO request)
    {
        try
        {
            var validationError = ValidateRankingEvent(request);
            if (validationError != null)
                return CustomResponse<RankingEventDTO>.Fail(validationError);

            var result = await _pointRepository.UpdateRankingEventAsync(id, request);
            if (result == null)
                return CustomResponse<RankingEventDTO>.Fail("Evento de ranking não encontrado.");

            var scheduleResponse = await ScheduleRankingEventAsync(id);

            if (!scheduleResponse.Success)
                _logger.LogWarning("Evento {EventId} atualizado, mas não foi possível reagendar o job.", id);

            return CustomResponse<RankingEventDTO>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar evento de ranking");
            return CustomResponse<RankingEventDTO>.Fail("Erro ao atualizar evento de ranking");
        }
    }

    public async Task<CustomResponse<bool>> DeleteRankingEventAsync(int id)
    {
        try
        {
            var deletedEvent = await _pointRepository.DeleteRankingEventAsync(id);
            if (deletedEvent == null)
                return CustomResponse<bool>.Fail("Evento de ranking não encontrado.");

            if (!string.IsNullOrWhiteSpace(deletedEvent.ScheduledJobId))
                BackgroundJob.Delete(deletedEvent.ScheduledJobId);

            return CustomResponse<bool>.SuccessTrade(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover evento de ranking");
            return CustomResponse<bool>.Fail("Erro ao remover evento de ranking");
        }
    }

    public async Task<CustomResponse<bool>> ScheduleRankingEventAsync(int eventId)
    {
        try
        {
            var rankingEvent = await _pointRepository.ScheduleRankingEventAsync(eventId);

            if (rankingEvent == null)
                return CustomResponse<bool>.Fail("Evento de ranking não encontrado.");

            var endTime = rankingEvent.StartTime.AddMinutes(rankingEvent.DurationMinutes);
            var scheduleAt = endTime;

            if (!string.IsNullOrWhiteSpace(rankingEvent.ScheduledJobId))
                BackgroundJob.Delete(rankingEvent.ScheduledJobId);

            var scheduledJobId = scheduleAt <= DateTime.UtcNow
                ? BackgroundJob.Enqueue<RankingEventJob>(
                    job => job.ProcessRankingRewardsAsync(eventId))
                : BackgroundJob.Schedule<RankingEventJob>(
                    job => job.ProcessRankingRewardsAsync(eventId),
                    scheduleAt);

            await _pointRepository.UpdateRankingEventScheduledJobIdAsync(eventId, scheduledJobId);

            return CustomResponse<bool>.SuccessTrade(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao agendar evento de ranking");
            return CustomResponse<bool>.Fail("Erro ao agendar evento de ranking");
        }
    }

    private static string? ValidateRankingEvent(RankingEventUpsertDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.EventName))
            return "Nome do evento é obrigatório.";

        if (!Enum.IsDefined(typeof(EventType), request.EventType))
            return "Tipo do evento inválido.";

        if (request.DurationMinutes <= 0)
            return "Duração do evento deve ser maior que zero.";

        if (request.Awards.Any(a => string.IsNullOrWhiteSpace(a.AwardName)))
            return "Nome do prêmio é obrigatório.";

        if (request.Awards.Any(a => a.AwardPositionRanking <= 0))
            return "Posição do prêmio deve ser maior que zero.";

        return null;
    }
}
