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

    public async Task<List<RankingPerCategoryDTO>> GetAvailableRankingPerCategoryAsync()
    {
        var allCategories = await _efDbContext.Categories
            .AsNoTracking()
            .ToListAsync();

        var categoryRankings = new List<RankingPerCategoryDTO>();

        foreach (var category in allCategories)
        {
            var exerciseIds = await _efDbContext.Exercises
                .AsNoTracking()
                .Where(exercise => exercise.CategoryId == category.Id)
                .Select(exercise => exercise.Id)
                .ToListAsync();

            if (!exerciseIds.Any())
                continue;

            var totalExercises = exerciseIds.Count;

            // Distinct exercises answered per user (no repetition by ExerciseId)
            var userAnswerCounts = await _efDbContext.Answers
                .AsNoTracking()
                .Where(answer => exerciseIds.Contains(answer.ExerciseId))
                .GroupBy(answer => new { answer.UserId, answer.ExerciseId })
                .Select(g => g.Key.UserId)
                .GroupBy(userId => userId)
                .Select(g => new { UserId = g.Key, TotalAnswers = g.Count() })
                .ToListAsync();

            if (!userAnswerCounts.Any())
                continue;

            var userIds = userAnswerCounts.Select(u => u.UserId).ToList();

            var users = await _efDbContext.Users
                .AsNoTracking()
                .Where(user => userIds.Contains(user.Id))
                .Select(user => new { user.Id, user.Name, user.ProfilePictureUrl })
                .ToListAsync();

            var rankings = userAnswerCounts
                .Select(u =>
                {
                    var user = users.FirstOrDefault(x => x.Id == u.UserId);
                    return new CategoryRankingDTO
                    {
                        UserId = u.UserId,
                        Name = user != null && !string.IsNullOrWhiteSpace(user.Name)
                            ? user.Name
                            : $"Aluno {u.UserId}",
                        ProfilePictureUrl = user?.ProfilePictureUrl,
                        TotalAnswers = u.TotalAnswers,
                        PercentAvailable = (float)Math.Round(
                            (double)u.TotalAnswers / totalExercises * 100, 2),
                    };
                })
                .OrderByDescending(r => r.TotalAnswers)
                .ToList();

            categoryRankings.Add(new RankingPerCategoryDTO
            {
                Category = category.Name ?? string.Empty,
                Rankings = rankings
            });
        }

        return categoryRankings;
    }

    public async Task<List<EventRankingDTO>> GetRankingEventAsync(int eventType)
    {
        var now = DateTime.UtcNow;

        var getCurrentEventAvailables = await _efDbContext.RankingEvents
            .AsNoTracking()
            .Where(e =>
                e.StartTime <= now &&
                e.StartTime.AddMinutes(e.DurationMinutes) >= now &&
                e.EventType == (EventType)eventType)
            .ToListAsync();

        var rewardsFromEvent = await _efDbContext.RankingAwards
            .AsNoTracking()
            .Where(r => getCurrentEventAvailables.Select(e => e.Id).Contains(r.EventId))
            .ToListAsync();

        return getCurrentEventAvailables.Select(e => new EventRankingDTO
        {
            EventName = e.EventName,
            EventType = e.EventType.ToString(),
            DurationMinutes = e.DurationMinutes,
            StartTime = e.StartTime,
            EventRankingAwards = rewardsFromEvent
                .Where(r => r.EventId == e.Id)
                .Select(r => new EventRankingAwardDTO
                {
                    AwardName = r.AwardName,
                    AwardPositionRanking = r.AwardPositionRanking,
                    AwardDescription = r.AwardDescription,
                    AwardPictureUrl = r.AwardPictureUrl
                })
                .OrderBy(r => r.AwardPositionRanking)
                .ToList() ?? new List<EventRankingAwardDTO>()
        }).ToList();
    }

    public async Task<RankingEvent?> ScheduleRankingEventAsync(int eventId)
    {
        var eventToSchedule = await _efDbContext.RankingEvents
            .FirstOrDefaultAsync(e => e.Id == eventId);

        return eventToSchedule;
    }
}
