using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API_PortalSantosTech.Repository;

public class ExerciseRepository : IExerciseRepository
{
    private readonly AppDbContext _efDbContext;

    public ExerciseRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<Exercise>> GetAllAsync()
    {
        return await _efDbContext.Exercises.AsNoTracking().ToListAsync();
    }

    public async Task<Exercise?> GetByIdAsync(int id)
    {
        return await _efDbContext.Exercises.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<ExerciseDailyTasksDTO>> GetDailyTasksForPhaseAsync(int phaseId)
    {
        var tasksContainer = await _efDbContext.DailyTasks
            .Where(dt => dt.PhaseId == phaseId)
            .AsNoTracking()
            .ToListAsync();

        var exerciseIds = tasksContainer
            .Select(task => task.ExerciseId)
            .Distinct()
            .ToList();

        var exercises = await _efDbContext.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .AsNoTracking()
            .ToListAsync();

        return tasksContainer
            .GroupBy(task => new { task.Name, task.PhaseId })
            .Select(group =>
            {
                return new ExerciseDailyTasksDTO
                {
                    Id = group.Min(task => task.Id),
                    Name = group.Key.Name ?? string.Empty,
                    PhaseId = group.Key.PhaseId,
                    Exercises = group
                        .Join(
                            exercises,
                            task => task.ExerciseId,
                            exercise => exercise.Id,
                            (_, exercise) => new ExerciseDTO
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
                                IsFinalExercise = exercise.IsFinalExercise
                            }
                        )
                        .OrderBy(exercise => exercise.IndexOrder)
                        .ToList()
                };
            })
            .OrderBy(taskGroup => taskGroup.Id)
            .ToList();
    }

    public async Task<List<QuestionOptionsDTO>> GetQuestionsOptionsForExerciseAsync(int exerciseId)
    {
        var questions = await _efDbContext.Questions
            .Where(q => q.ExerciseId == exerciseId)
            .AsNoTracking()
            .ToListAsync();

        var questionIds = questions.Select(q => q.Id).ToList();

        return await _efDbContext.QuestionOptions
            .Where(qo => questionIds.Contains(qo.QuestionId))
            .AsNoTracking()
            .Select(qo => new QuestionOptionsDTO
            {
                Id = qo.Id,
                QuestionId = qo.QuestionId,
                Question = qo.OptionText,
                CorrectAnswer = qo.IsCorrect
            })
            .ToListAsync();
    }

    public async Task<bool> SubmitExerciseAnswersAsync(ExerciseSubmissionDTO submission)
    {
        await _efDbContext.Answers.AddAsync(new Answer
        {
            UserId = submission.UserId,
            QuestionId = submission.QuestionId,
            ExerciseId = submission.ExerciseId,
            AnswerText = null,
            AnsweredAt = DateTime.UtcNow
        });

        await _efDbContext.SaveChangesAsync();
        return true;
    }
}
