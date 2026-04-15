using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class ProgressStudentPhaseService : IProgressService
{
    private readonly ILogger<ProgressStudentPhaseService> _logger;
    private readonly IProgressRepository _progressRepository;
    private readonly ILogsRepository _logsRepository;

    public ProgressStudentPhaseService(
        ILogger<ProgressStudentPhaseService> logger,
        IProgressRepository progressRepository,
        ILogsRepository logsRepository)
    {
        _logger = logger;
        _progressRepository = progressRepository;
        _logsRepository = logsRepository;
    }

    public async Task<CustomResponse<IEnumerable<ProgressStudentPhase>>> GetAllStudentPhasesAsync()
    {
        var result = await _progressRepository.GetAllStudentPhasesAsync();
        return CustomResponse<IEnumerable<ProgressStudentPhase>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<IEnumerable<ProgressExerciseStudent>>> GetAllExerciseAsync()
    {
        var result = await _progressRepository.GetAllExerciseAsync();
        return CustomResponse<IEnumerable<ProgressExerciseStudent>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<IEnumerable<ProgressVideoStudent>>> GetAllVideoStudentsAsync()
    {
        var result = await _progressRepository.GetAllVideoStudentsAsync();
        return CustomResponse<IEnumerable<ProgressVideoStudent>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<ProgressStudentPhase>> GetStudentPhaseByIdAsync(int id)
    {
        var result = await _progressRepository.GetStudentPhaseByIdAsync(id);
        return result == null
            ? CustomResponse<ProgressStudentPhase>.Fail("Progress student phase not found")
            : CustomResponse<ProgressStudentPhase>.SuccessTrade(result);
    }

    public async Task<CustomResponse<ProgressExerciseStudent>> GetExerciseByIdAsync(int id)
    {
        var result = await _progressRepository.GetExerciseByIdAsync(id);
        return result == null
            ? CustomResponse<ProgressExerciseStudent>.Fail("Progress exercise student not found")
            : CustomResponse<ProgressExerciseStudent>.SuccessTrade(result);
    }

    public async Task<CustomResponse<IEnumerable<ProgressPaidCourses>>> GetProgressUserPaidCoursesAsync(int userId)
    {
        try
        {
            var result = await _progressRepository.GetProgressUserPaidCoursesAsync(userId);
            return result == null ? CustomResponse<IEnumerable<ProgressPaidCourses>>.Fail("Nenhum progresso encontrado") : CustomResponse<IEnumerable<ProgressPaidCourses>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao buscar progresso dos cursos pagos do usuário com ID {userId}");
            return CustomResponse<IEnumerable<ProgressPaidCourses>>.Fail("Erro ao buscar progresso dos cursos pagos do usuário");
        }
    }

    public async Task<CustomResponse<IEnumerable<VideoProgressDTO>>> GetProgressUserVideosAsync(int userId)
    {
        var result = await _progressRepository.GetProgressUserVideosAsync(userId);
        return result == null
            ? CustomResponse<IEnumerable<VideoProgressDTO>>.Fail("Nenhum progresso de vídeo encontrado")
            : CustomResponse<IEnumerable<VideoProgressDTO>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<ProgressVideoStudent>> GetVideoStudentByIdAsync(int id)
    {
        var result = await _progressRepository.GetVideoStudentByIdAsync(id);
        return result == null
            ? CustomResponse<ProgressVideoStudent>.Fail("Progress video student not found")
            : CustomResponse<ProgressVideoStudent>.SuccessTrade(result);
    }

    public async Task<CustomResponse<VideoProgressDTO>> SaveProgressVideoAsync(VideoProgressDTO progressData)
    {
        try
        {
            var existingProgress = await _progressRepository.GetProgressUserVideosAsync(progressData.UserId);
            var progressToUpdate = existingProgress.FirstOrDefault(p => p.VideoId == progressData.VideoId);

            if (progressToUpdate != null)
            {
                var update = await _progressRepository.UpdateProgressVideoAsync(progressData);

                await _logsRepository.AddLogsAsync(new Logs
                {
                    UserId = progressData.UserId,
                    Action = $"Update de progresso em video ID: {progressData.VideoId}",
                    Message = $"Progresso de Update: WatchedSeconds={update.WatchSeconds}, IsCompleted={update.IsCompleted}",
                    EntityName = "ProgressVideoStudent",
                    LogDate = DateTime.UtcNow
                });
            }
            else
            {
                await _progressRepository.AddProgressVideoAsync(progressData);

                await _logsRepository.AddLogsAsync(new Logs
                {
                    UserId = progressData.UserId,
                    Action = $"Adicionado progresso de video para video ID: {progressData.VideoId}",
                    Message = $"Progresso Adicionado: WatchedSeconds={progressData.WatchSeconds}, IsCompleted={progressData.IsCompleted}",
                    EntityName = "ProgressVideoStudent",
                    LogDate = DateTime.UtcNow
                });
            }

            return CustomResponse<VideoProgressDTO>.SuccessTrade(progressData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving video progress for user ID: {UserId} and video ID: {VideoId}", progressData.UserId, progressData.VideoId);
            return CustomResponse<VideoProgressDTO>.Fail("An error occurred while saving video progress");
        }
    }

    public async Task<CustomResponse<bool>> UpdateGoalProgressAsync(int userId, int goalType, int rewardType)
    {
        try
        {
            var result = await _progressRepository.UpdateGoalProgressAsync(userId, goalType, rewardType);

            return CustomResponse<bool>.SuccessTrade(result != null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating goal progress for user ID: {UserId} and goal type: {GoalType}", userId, goalType);
            return CustomResponse<bool>.Fail("An error occurred while updating goal progress");
        }
    }

    public async Task<CustomResponse<bool>> EvaluateProgressAsync(int userId, int goalRewardId, int rewardType)
    {
        try
        {
            var result = await _progressRepository.EvaluateProgress(userId, goalRewardId, rewardType);

            return CustomResponse<bool>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao avaliar progresso para o usuário ID: {UserId} e goal student ID: {GoalStudentId}", userId, goalRewardId);
            return CustomResponse<bool>.Fail("Ocorreu um erro ao avaliar o progresso");
        }
    }
}
