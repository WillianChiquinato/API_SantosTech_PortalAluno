using API_PortalSantosTech.Data;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Jobs;

public class RankingEventJob
{
    private readonly AppDbContext _db;
    private readonly ILogger<RankingEventJob> _logger;

    public RankingEventJob(AppDbContext db, ILogger<RankingEventJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task ProcessRankingRewardsAsync(int eventId)
    {
        var rankingEvent = await _db.RankingEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (rankingEvent == null)
        {
            _logger.LogWarning("RankingEventJob: Evento {EventId} não encontrado.", eventId);
            return;
        }

        var awards = await _db.RankingAwards
            .AsNoTracking()
            .Where(a => a.EventId == eventId)
            .OrderBy(a => a.AwardPositionRanking)
            .ToListAsync();

        if (!awards.Any())
        {
            _logger.LogInformation("RankingEventJob: Nenhum prêmio configurado para o evento {EventId}.", eventId);
            return;
        }

        List<(int UserId, decimal Score)> rawRanking;

        if (rankingEvent.EventType == EventType.Notas)
        {
            // Carrega em memória para evitar limitações de tradução EF Core no GroupBy complexo
            var allAnswers = await _db.Answers
            .AsNoTracking()
                .Select(a => new { a.UserId, a.ExerciseId, a.QuestionId, a.IsCorrect, a.AnsweredAt })
                .ToListAsync();

            rawRanking = allAnswers
                .GroupBy(a => new { a.UserId, a.ExerciseId, a.QuestionId })
                .Select(g => g.OrderByDescending(a => a.AnsweredAt).First())
                .GroupBy(a => a.UserId)
                .Select(g =>
                {
                    var totalAnswers = g.Count();
                    var correctAnswers = g.Count(a => a.IsCorrect);
                    var percentage = totalAnswers == 0 || totalAnswers < 10 ? 0m : (decimal)correctAnswers / totalAnswers * 100m;
                    return (UserId: g.Key, Score: percentage);
                })
                .OrderByDescending(x => x.Score)
                .ToList();
        }
        else
        {
            var pointTotals = await _db.Points
            .AsNoTracking()
            .GroupBy(p => p.UserId)
            .Select(g => new { UserId = g.Key, Score = (long)g.Sum(p => p.Points) })
            .OrderByDescending(x => x.Score)
            .ToListAsync();

            rawRanking = pointTotals
                .Select(x => (UserId: x.UserId, Score: (decimal)x.Score))
                .ToList();
        }

        if (!rawRanking.Any())
        {
            _logger.LogInformation("RankingEventJob: Nenhum usuário no ranking para o evento {EventId}.", eventId);
            return;
        }

        var now = DateTime.UtcNow;
        var historyEntries = new List<RankingHistory>();

        foreach (var award in awards)
        {
            var position = award.AwardPositionRanking;
            if (position < 1 || position > rawRanking.Count)
                continue;

            var winner = rawRanking[position - 1];

            historyEntries.Add(new RankingHistory
            {
                UserId = winner.UserId,
                EventId = eventId,
                AwardId = award.Id,
                RankingPosition = position,
                RecordedAt = now
            });
        }

        if (historyEntries.Any())
        {
            await _db.RankingHistories.AddRangeAsync(historyEntries);
            await _db.SaveChangesAsync();
            _logger.LogInformation(
                "RankingEventJob: {Count} recompensa(s) registrada(s) para o evento {EventId}.",
                historyEntries.Count, eventId);
        }
    }
}

