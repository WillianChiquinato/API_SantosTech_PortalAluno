using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class BadgeService : IBadgeService
{
    private readonly ILogger<BadgeService> _logger;
    private readonly IBadgeRepository _badgeRepository;

    public BadgeService(ILogger<BadgeService> logger, IBadgeRepository badgeRepository)
    {
        _logger = logger;
        _badgeRepository = badgeRepository;
    }

    public async Task<CustomResponse<IEnumerable<ActivatedGoalResponse>>> GetActivatedGoalsByUserAndCourseIdAsync(int userId, int courseId)
    {
        try
        {
            var result = await _badgeRepository.GetActivatedGoalsByUserAndCourseIdAsync(userId, courseId);
            
            return CustomResponse<IEnumerable<ActivatedGoalResponse>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar metas ativadas para o usuário ID {UserId} e curso ID {CourseId}", userId, courseId);
            return CustomResponse<IEnumerable<ActivatedGoalResponse>>.Fail("Ocorreu um erro ao buscar os dados");
        }
    }

    public async Task<CustomResponse<IEnumerable<Badge>>> GetAllAsync()
    {
        try
        {
            var result = await _badgeRepository.GetAllAsync();
            return CustomResponse<IEnumerable<Badge>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todas as badges");
            return CustomResponse<IEnumerable<Badge>>.Fail("Ocorreu um erro ao buscar os dados");
        }
    }

    public async Task<CustomResponse<Badge>> GetByIdAsync(int id)
    {
        try
        {
            var result = await _badgeRepository.GetByIdAsync(id);
            if (result == null)
                return CustomResponse<Badge>.Fail("Badge não encontrado");

            return CustomResponse<Badge>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar badge por ID {BadgeId}", id);
            return CustomResponse<Badge>.Fail("Ocorreu um erro ao buscar os dados");
        }
    }

    public async Task<CustomResponse<IEnumerable<GoalWithBadgesResponse>>> GetGoalsWithBadgesByCourseIdAsync(int courseId)
    {
        try
        {
            var result = await _badgeRepository.GetGoalsWithBadgesByCourseIdAsync(courseId);
            
            return CustomResponse<IEnumerable<GoalWithBadgesResponse>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar metas com badges para o curso ID {CourseId}", courseId);
            return CustomResponse<IEnumerable<GoalWithBadgesResponse>>.Fail("Ocorreu um erro ao buscar os dados");
        }
    }

    public async Task<CustomResponse<bool>> GoalRewardOperationAsync(int goalRewardId, int userId)
    {
        try
        {
            var result = await _badgeRepository.GoalRewardOperationAsync(goalRewardId, userId);

            if (result)
                return CustomResponse<bool>.SuccessTrade(true);
            else
                return CustomResponse<bool>.Fail("Falha ao realizar a operação de recompensa");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar a operação de recompensa para GoalReward ID {GoalRewardId} e User ID {UserId}", goalRewardId, userId);
            return CustomResponse<bool>.Fail("Ocorreu um erro ao realizar a operação");
        }
    }

    public async Task<CustomResponse<bool>> UpdateActivatedGoalIdAsync(int goalRewardId, int userId)
    {
        try
        {
            var updateResult = await _badgeRepository.UpdateActivatedGoalIdAsync(goalRewardId, userId);

            if (updateResult)
                return CustomResponse<bool>.SuccessTrade(true);
            else
                return CustomResponse<bool>.Fail("Falha ao atualizar o goalId ativado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar o goalId ativado para Goal ID {GoalId}", goalRewardId);
            return CustomResponse<bool>.Fail("Ocorreu um erro ao atualizar os dados");
        }
    }
}
