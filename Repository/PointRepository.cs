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

    public async Task<List<RankingCategoryDTO>> GetRankingCategoriesAsync()
    {
        return await _efDbContext.Categories
            .AsNoTracking()
            .Where(category => _efDbContext.Exercises.Any(exercise => exercise.CategoryId == category.Id))
            .OrderBy(category => category.Name)
            .Select(category => new RankingCategoryDTO
            {
                Id = category.Id,
                Category = category.Name ?? string.Empty
            })
            .ToListAsync();
    }

    public async Task<(List<RankingPerCategoryDTO> Items, int TotalRows)> GetAvailableRankingPerCategoryAsync(string? category, int? limit, int offset)
    {
        var categoriesQuery = _efDbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalizedCategory = category.Trim();
            categoriesQuery = categoriesQuery.Where(c => c.Name == normalizedCategory);
        }

        var allCategories = await categoriesQuery.ToListAsync();

        var categoryRankings = new List<RankingPerCategoryDTO>();
        var totalRows = 0;

        foreach (var currentCategory in allCategories)
        {
            var exerciseIds = await _efDbContext.Exercises
                .AsNoTracking()
                .Where(exercise => exercise.CategoryId == currentCategory.Id)
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

            totalRows += userAnswerCounts.Count;

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
                .ThenBy(r => r.Name)
                .Skip(limit.HasValue ? offset : 0)
                .Take(limit ?? int.MaxValue)
                .ToList();

            categoryRankings.Add(new RankingPerCategoryDTO
            {
                Category = currentCategory.Name ?? string.Empty,
                Rankings = rankings
            });
        }

        return (categoryRankings, totalRows);
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
            Id = e.Id,
            EventName = e.EventName,
            EventType = e.EventType.ToString(),
            DurationMinutes = e.DurationMinutes,
            StartTime = e.StartTime,
            EventRankingAwards = rewardsFromEvent
                .Where(r => r.EventId == e.Id)
                .Select(r => new EventRankingAwardDTO
                {
                    Id = r.Id,
                    AwardName = r.AwardName,
                    AwardPositionRanking = r.AwardPositionRanking,
                    AwardDescription = r.AwardDescription,
                    AwardPictureUrl = r.AwardPictureUrl
                })
                .OrderBy(r => r.AwardPositionRanking)
                .ToList() ?? new List<EventRankingAwardDTO>()
        }).ToList();
    }

    public async Task<(List<RankingEventHistoryDTO> Items, int TotalRows)> GetRankingEventHistoryAsync(int? eventType, int limit, int offset)
    {
        var query = _efDbContext.RankingHistories
            .AsNoTracking()
            .Include(h => h.Event)
            .Include(h => h.Award)
            .Include(h => h.User)
            .AsQueryable();

        if (eventType.HasValue)
            query = query.Where(h => h.Event.EventType == (EventType)eventType.Value);

        var totalRows = await query.CountAsync();

        var items = await query
            .OrderByDescending(h => h.RecordedAt)
            .ThenBy(h => h.RankingPosition)
            .Skip(offset)
            .Take(limit)
            .Select(h => new RankingEventHistoryDTO
            {
                Id = h.Id,
                EventId = h.EventId,
                EventName = h.Event.EventName,
                EventType = h.Event.EventType.ToString(),
                UserId = h.UserId,
                UserName = !string.IsNullOrWhiteSpace(h.User.Name)
                    ? h.User.Name
                    : $"Aluno {h.UserId}",
                UserProfilePictureUrl = h.User.ProfilePictureUrl,
                AwardId = h.AwardId,
                AwardName = h.Award.AwardName,
                AwardDescription = h.Award.AwardDescription,
                AwardPictureUrl = h.Award.AwardPictureUrl,
                RankingPosition = h.RankingPosition,
                RecordedAt = h.RecordedAt
            })
            .ToListAsync();

        return (items, totalRows);
    }

    public async Task<EventRankingDTO?> GetRankingEventByIdAsync(int id)
    {
        var rankingEvent = await _efDbContext.RankingEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (rankingEvent == null)
            return null;

        var awards = await _efDbContext.RankingAwards
            .AsNoTracking()
            .Where(a => a.EventId == id)
            .OrderBy(a => a.AwardPositionRanking)
            .ToListAsync();

        return new EventRankingDTO
        {
            Id = rankingEvent.Id,
            EventName = rankingEvent.EventName,
            EventType = rankingEvent.EventType.ToString(),
            DurationMinutes = rankingEvent.DurationMinutes,
            StartTime = rankingEvent.StartTime,
            EventRankingAwards = awards.Select(a => new EventRankingAwardDTO
            {
                Id = a.Id,
                AwardName = a.AwardName,
                AwardPositionRanking = a.AwardPositionRanking,
                AwardDescription = a.AwardDescription,
                AwardPictureUrl = a.AwardPictureUrl
            }).ToList()
        };
    }

    public async Task<RankingEventDTO> CreateRankingEventAsync(RankingEventUpsertDTO request)
    {
        var rankingEvent = new RankingEvent
        {
            EventName = request.EventName.Trim(),
            EventType = (EventType)request.EventType,
            DurationMinutes = request.DurationMinutes,
            StartTime = NormalizeUtc(request.StartTime)
        };

        _efDbContext.RankingEvents.Add(rankingEvent);
        await _efDbContext.SaveChangesAsync();

        var awards = request.Awards.Select(award => new RankingAward
        {
            EventId = rankingEvent.Id,
            AwardName = award.AwardName.Trim(),
            AwardPositionRanking = award.AwardPositionRanking,
            AwardDescription = award.AwardDescription.Trim(),
            AwardPictureUrl = award.AwardPictureUrl.Trim()
        }).ToList();

        if (awards.Any())
        {
            _efDbContext.RankingAwards.AddRange(awards);
            await _efDbContext.SaveChangesAsync();
        }

        return MapRankingEventDto(rankingEvent, awards);
    }

    public async Task<RankingEventDTO?> UpdateRankingEventAsync(int id, RankingEventUpsertDTO request)
    {
        var rankingEvent = await _efDbContext.RankingEvents
            .FirstOrDefaultAsync(e => e.Id == id);

        if (rankingEvent == null)
            return null;

        rankingEvent.EventName = request.EventName.Trim();
        rankingEvent.EventType = (EventType)request.EventType;
        rankingEvent.DurationMinutes = request.DurationMinutes;
        rankingEvent.StartTime = NormalizeUtc(request.StartTime);

        var currentAwards = await _efDbContext.RankingAwards
            .Where(a => a.EventId == id)
            .ToListAsync();

        _efDbContext.RankingAwards.RemoveRange(currentAwards);

        var newAwards = request.Awards.Select(award => new RankingAward
        {
            EventId = id,
            AwardName = award.AwardName.Trim(),
            AwardPositionRanking = award.AwardPositionRanking,
            AwardDescription = award.AwardDescription.Trim(),
            AwardPictureUrl = award.AwardPictureUrl.Trim()
        }).ToList();

        if (newAwards.Any())
            _efDbContext.RankingAwards.AddRange(newAwards);

        await _efDbContext.SaveChangesAsync();

        return MapRankingEventDto(rankingEvent, newAwards);
    }

    public async Task<RankingEvent?> DeleteRankingEventAsync(int id)
    {
        var rankingEvent = await _efDbContext.RankingEvents
            .FirstOrDefaultAsync(e => e.Id == id);

        if (rankingEvent == null)
            return null;

        var awards = await _efDbContext.RankingAwards
            .Where(a => a.EventId == id)
            .ToListAsync();

        _efDbContext.RankingAwards.RemoveRange(awards);
        _efDbContext.RankingEvents.Remove(rankingEvent);
        await _efDbContext.SaveChangesAsync();

        return rankingEvent;
    }

    public async Task<RankingEvent?> ScheduleRankingEventAsync(int eventId)
    {
        var eventToSchedule = await _efDbContext.RankingEvents
            .FirstOrDefaultAsync(e => e.Id == eventId);

        return eventToSchedule;
    }

    public async Task UpdateRankingEventScheduledJobIdAsync(int eventId, string? scheduledJobId)
    {
        var rankingEvent = await _efDbContext.RankingEvents
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (rankingEvent == null)
            return;

        rankingEvent.ScheduledJobId = scheduledJobId;
        await _efDbContext.SaveChangesAsync();
    }

    private static RankingEventDTO MapRankingEventDto(RankingEvent rankingEvent, IEnumerable<RankingAward> awards)
    {
        return new RankingEventDTO
        {
            Id = rankingEvent.Id,
            EventName = rankingEvent.EventName,
            EventType = (int)rankingEvent.EventType,
            DurationMinutes = rankingEvent.DurationMinutes,
            StartTime = rankingEvent.StartTime,
            Awards = awards
                .OrderBy(a => a.AwardPositionRanking)
                .Select(a => new RankingAwardDTO
                {
                    Id = a.Id,
                    AwardName = a.AwardName,
                    AwardPositionRanking = a.AwardPositionRanking,
                    AwardDescription = a.AwardDescription,
                    AwardPictureUrl = a.AwardPictureUrl
                })
                .ToList()
        };
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
