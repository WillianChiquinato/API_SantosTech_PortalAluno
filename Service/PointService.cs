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

    public async Task<CustomResponse<IEnumerable<RankingPerCategoryDTO>>> GetAvailableRankingPerCategoryAsync()
    {
        try
        {
            var result = await _pointRepository.GetAvailableRankingPerCategoryAsync();
            return CustomResponse<IEnumerable<RankingPerCategoryDTO>>.SuccessTrade(result);
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

    public async Task<CustomResponse<bool>> ScheduleRankingEventAsync(int eventId)
    {
        try
        {
            var rankingEvent = await _pointRepository.ScheduleRankingEventAsync(eventId);

            if (rankingEvent == null)
                return CustomResponse<bool>.Fail("Evento de ranking não encontrado.");

            var endTime = rankingEvent.StartTime.AddMinutes(rankingEvent.DurationMinutes);
            var scheduleAt = endTime.AddDays(1);

            BackgroundJob.Schedule<RankingEventJob>(
                job => job.ProcessRankingRewardsAsync(eventId),
                scheduleAt);

            return CustomResponse<bool>.SuccessTrade(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao agendar evento de ranking");
            return CustomResponse<bool>.Fail("Erro ao agendar evento de ranking");
        }
    }
}
