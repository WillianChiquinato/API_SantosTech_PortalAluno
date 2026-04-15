using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class PointRepository : IPointRepository
{
    private readonly AppDbContext _efDbContext;

    public PointRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<Point>> GetAllAsync()
    {
        return await _efDbContext.Points.AsNoTracking().ToListAsync();
    }

    public async Task<Point?> GetByIdAsync(int id)
    {
        return await _efDbContext.Points.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<List<PointRankingDTO>> GetRankingAsync()
    {
        var pointTotals = _efDbContext.Points
            .AsNoTracking()
            .GroupBy(p => p.UserId)
            .Select(g => new Point
            {
                UserId = g.Key,
                Points = g.Sum(p => p.Points)
            });

        return pointTotals
            .GroupJoin(
                _efDbContext.Users.AsNoTracking(),
                point => point.UserId,
                user => user.Id,
                (point, users) => new { point, users })
            .SelectMany(
                result => result.users.DefaultIfEmpty(),
                (result, user) => new PointRankingDTO
                {
                    UserId = result.point.UserId,
                    TotalPoints = result.point.Points,
                    Name = !string.IsNullOrWhiteSpace(user != null ? user.Name : null)
                        ? user!.Name!
                        : $"Aluno {result.point.UserId}",
                    ProfilePictureUrl = user != null ? user.ProfilePictureUrl : null
                })
            .OrderByDescending(p => p.TotalPoints)
            .ToListAsync();
    }

    public async Task<ExercisePointAwardResult> AddPointsForUserAsync(int userId, int exerciseId)
    {
        var exercise = await _efDbContext.Exercises
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == exerciseId);

        if (exercise == null)
            return new ExercisePointAwardResult { Success = false };

        var alreadyAwarded = await _efDbContext.Points
            .AsNoTracking()
            .AnyAsync(point =>
                point.UserId == userId &&
                point.Reason != null &&
                point.Reason.StartsWith($"exercise:{exerciseId};"));

        if (alreadyAwarded)
        {
            return new ExercisePointAwardResult
            {
                Success = true,
                AlreadyAwarded = true,
                PointsAwarded = 0
            };
        }

        var latestAnswers = await _efDbContext.Answers
            .Where(answer => answer.UserId == userId && answer.ExerciseId == exerciseId)
            .OrderByDescending(answer => answer.AnsweredAt)
            .ToListAsync();

        if (!latestAnswers.Any())
            return new ExercisePointAwardResult { Success = false };

        var latestAnswersByQuestion = latestAnswers
            .GroupBy(answer => answer.QuestionId)
            .Select(group => group.First())
            .ToList();

        var totalQuestions = await _efDbContext.Questions
            .AsNoTracking()
            .CountAsync(question => question.ExerciseId == exerciseId);

        if (totalQuestions <= 0)
            return new ExercisePointAwardResult { Success = false };

        var correctAnswers = latestAnswersByQuestion.Count(answer => answer.IsCorrect);
        var calculatedPoints = (int)Math.Round(
            exercise.PointsRedeem * ((double)correctAnswers / totalQuestions),
            MidpointRounding.AwayFromZero);

        if (exercise.TermAt.Date < DateTime.UtcNow.Date)
        {
            calculatedPoints = (int)Math.Round(
                calculatedPoints * 0.6,
                MidpointRounding.AwayFromZero);
        }

        if (calculatedPoints <= 0)
        {
            return new ExercisePointAwardResult
            {
                Success = true,
                PointsAwarded = 0
            };
        }

        var reason = $"exercise:{exerciseId}; Score={correctAnswers}/{totalQuestions}; Points={calculatedPoints}";

        _efDbContext.Points.Add(new Point
        {
            UserId = userId,
            Points = calculatedPoints,
            Reason = reason,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _efDbContext.SaveChangesAsync();

        return new ExercisePointAwardResult
        {
            Success = true,
            PointsAwarded = calculatedPoints
        };
    }

    public async Task<Point> AddPointsAsync(Point point)
    {
        _efDbContext.Points.Add(point);
        await _efDbContext.SaveChangesAsync();
        
        return point;
    }
}
