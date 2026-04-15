using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IProgressRepository
{
    //Student progress in exercises
    Task<List<ProgressExerciseStudent>> GetAllExerciseAsync();
    Task<ProgressExerciseStudent?> GetExerciseByIdAsync(int id);
    
    //Student progress in phases
    Task<List<ProgressStudentPhase>> GetAllStudentPhasesAsync();
    Task<ProgressStudentPhase?> GetStudentPhaseByIdAsync(int id);

    //User progress in videos
    Task<List<ProgressVideoStudent>> GetAllVideoStudentsAsync();
    Task<ProgressVideoStudent?> GetVideoStudentByIdAsync(int id);
    Task<List<VideoProgressDTO>> GetProgressUserVideosAsync(int userId);
    Task<VideoProgressDTO> UpdateProgressVideoAsync(VideoProgressDTO progressData);
    Task<VideoProgressDTO> AddProgressVideoAsync(VideoProgressDTO progressData);

    // Goal progress
    Task<ProgressStudentPhase?> GetProgressByUserIdAndPhaseIdAsync(int userId, int phaseId);
    Task<ProgressGoalStudent?> UpdateGoalProgressAsync(int userId, int goalType, int rewardType);
    Task<bool> EvaluateProgress(int userId, int goalStudentId, int rewardType);


    // Progress in paid courses
    Task<List<ProgressPaidCourses>> GetProgressUserPaidCoursesAsync(int userId);
    Task<bool> UpdateProgressAsync(int userId, int phaseId, int totalAnswered, int totalExercises);
}
