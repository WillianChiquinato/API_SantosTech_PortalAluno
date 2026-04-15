using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IProgressService
{
    Task<CustomResponse<IEnumerable<ProgressExerciseStudent>>> GetAllExerciseAsync();
    Task<CustomResponse<ProgressExerciseStudent>> GetExerciseByIdAsync(int id);
    Task<CustomResponse<IEnumerable<ProgressStudentPhase>>> GetAllStudentPhasesAsync();
    Task<CustomResponse<ProgressStudentPhase>> GetStudentPhaseByIdAsync(int id);
    Task<CustomResponse<IEnumerable<ProgressVideoStudent>>> GetAllVideoStudentsAsync();
    Task<CustomResponse<ProgressVideoStudent>> GetVideoStudentByIdAsync(int id);
    Task<CustomResponse<IEnumerable<VideoProgressDTO>>> GetProgressUserVideosAsync(int userId);
    Task<CustomResponse<VideoProgressDTO>> SaveProgressVideoAsync(VideoProgressDTO progressData);
    Task<CustomResponse<bool>> UpdateGoalProgressAsync(int userId, int goalType, int rewardType);
    Task<CustomResponse<IEnumerable<ProgressPaidCourses>>> GetProgressUserPaidCoursesAsync(int userId);
    Task<CustomResponse<bool>> EvaluateProgressAsync(int userId, int goalRewardId, int rewardType);
}
