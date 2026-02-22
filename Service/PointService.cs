using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

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
            _logger.LogError(ex, "Error fetching points");
            return CustomResponse<IEnumerable<Point>>.Fail("Error fetching points");
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
            _logger.LogError(ex, "Error fetching point by ID");
            return CustomResponse<Point>.Fail("Error fetching point by ID");
        }
    }

    public async Task<CustomResponse<IEnumerable<PointRankingDTO>>> GetRankingAsync()
    {
        try
        {
            var result = await _pointRepository.GetRankingAsync();
            var rankingDtos = result.Select(p => new PointRankingDTO 
            { 
                UserId = p.UserId, 
                TotalPoints = p.Points
            }).ToList();
            return CustomResponse<IEnumerable<PointRankingDTO>>.SuccessTrade(rankingDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching point ranking");
            return CustomResponse<IEnumerable<PointRankingDTO>>.Fail("Error fetching point ranking");
        }
    }

    public async Task<CustomResponse<string>> AddPointsForUserAsync(RedeemPointsDTO redeemPoints)
    {
        try
        {
            var resultPointsUser = await _pointRepository.AddPointsForUserAsync(redeemPoints.UserId, redeemPoints.PointsToRedeem);

            return CustomResponse<string>.SuccessTrade("Pontos resgatados com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao resgatar pontos para o usuário");
            return CustomResponse<string>.Fail("Erro ao resgatar pontos para o usuário");
        }
    }
}
