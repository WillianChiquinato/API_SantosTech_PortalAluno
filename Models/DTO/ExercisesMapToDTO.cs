using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;

public class ExercisesMapToDTO
{
    public ExerciseDTO MapToDTO(Exercise exercise)
    {
        return new ExerciseDTO
        {
            Id = exercise.Id,
            Title = exercise.Title,
            Description = exercise.Description,
            VideoUrl = exercise.VideoUrl,
            PointsRedeem = exercise.PointsRedeem,
            TermAt = exercise.TermAt,
            TypeExercise = exercise.TypeExercise,
            Difficulty = exercise.Difficulty,
            IndexOrder = exercise.IndexOrder,
            IsDailyTask = exercise.IsDailyTask,
            IsFinalExercise = exercise.IsFinalExercise,
            ExercisePeriod = exercise.ExercisePeriod
        };
    }
}