using System.Collections.Concurrent;
using System.Text.Json;
using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Services;

public class FinalChallengeRealtimeWorker : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<FinalChallengeRealtimeWorker> _logger;
    private readonly ConcurrentDictionary<int, EventRealtimeState> _eventStates = new();

    public FinalChallengeRealtimeWorker(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<FinalChallengeRealtimeWorker> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Erro ao publicar eventos em tempo real do desafio final.");
            }

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task PublishChangesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var challengeService = scope.ServiceProvider.GetRequiredService<ITeamsChallengerService>();
        var notifier = scope.ServiceProvider.GetRequiredService<IFinalChallengeRealtimeNotifier>();

        var candidateEventIds = await dbContext.FinalChallengeEvents
            .AsNoTracking()
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var liveEventIds = new HashSet<int>();

        foreach (var eventId in candidateEventIds)
        {
            var response = await challengeService.GetLiveSnapshotAsync(eventId, null);
            if (!response.Success || response.Result is null)
                continue;

            var snapshot = response.Result;
            liveEventIds.Add(eventId);

            var nextState = BuildState(snapshot);
            if (!_eventStates.TryGetValue(eventId, out var previousState))
            {
                _eventStates[eventId] = nextState;
                continue;
            }

            if (!string.Equals(previousState.SnapshotHash, nextState.SnapshotHash, StringComparison.Ordinal))
            {
                await notifier.PublishSnapshotUpdatedAsync(new FinalChallengeSnapshotUpdatedEvent
                {
                    EventId = eventId,
                    Snapshot = snapshot
                }, cancellationToken);
            }

            if (!string.Equals(previousState.LeaderboardHash, nextState.LeaderboardHash, StringComparison.Ordinal))
            {
                await notifier.PublishLeaderboardUpdatedAsync(new FinalChallengeLeaderboardUpdatedEvent
                {
                    EventId = eventId,
                    ServerTime = snapshot.Event.ServerTime,
                    Summary = snapshot.Summary,
                    Clans = snapshot.Clans
                }, cancellationToken);
            }

            foreach (var clan in snapshot.Clans)
            {
                if (!previousState.ClanRanks.TryGetValue(clan.ClanId, out var previousRank))
                    continue;

                if (!nextState.ClanRanks.TryGetValue(clan.ClanId, out var currentRank) || previousRank == currentRank)
                    continue;

                await notifier.PublishClanMovedAsync(new FinalChallengeClanMovedEvent
                {
                    EventId = eventId,
                    ClanId = clan.ClanId,
                    ClanName = clan.Name,
                    PreviousRank = previousRank,
                    CurrentRank = currentRank,
                    HappenedAt = snapshot.Event.ServerTime,
                    Clan = clan
                }, cancellationToken);
            }

            var newValidatedItems = snapshot.Feed
                .Where(item => string.Equals(item.Type, "validated", StringComparison.OrdinalIgnoreCase))
                .Where(item => !previousState.ValidatedFeedIds.Contains(item.Id))
                .ToList();

            foreach (var item in newValidatedItems)
            {
                await notifier.PublishChallengeValidatedAsync(new FinalChallengeChallengeValidatedEvent
                {
                    EventId = eventId,
                    HappenedAt = item.HappenedAt,
                    Item = item
                }, cancellationToken);
            }

            if (!string.Equals(previousState.EventStatus, "closed", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(nextState.EventStatus, "closed", StringComparison.OrdinalIgnoreCase))
            {
                await notifier.PublishWeekClosedAsync(new FinalChallengeWeekClosedEvent
                {
                    EventId = eventId,
                    ClosedAt = snapshot.Event.ServerTime,
                    Summary = snapshot.Summary,
                    FinalLeaderboard = snapshot.Clans
                }, cancellationToken);
            }

            _eventStates[eventId] = nextState;
        }

        var staleEventIds = _eventStates.Keys.Where(eventId => !liveEventIds.Contains(eventId)).ToList();
        foreach (var staleEventId in staleEventIds)
            _eventStates.TryRemove(staleEventId, out _);
    }

    private static EventRealtimeState BuildState(FinalChallengeSnapshot snapshot)
    {
        var clanRanks = snapshot.Clans
            .Select((clan, index) => new { clan.ClanId, Rank = index + 1 })
            .ToDictionary(x => x.ClanId, x => x.Rank);

        var validatedFeedIds = snapshot.Feed
            .Where(item => string.Equals(item.Type, "validated", StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new EventRealtimeState
        {
            EventStatus = snapshot.Event.Status,
            SnapshotHash = ComputeHash(snapshot),
            LeaderboardHash = ComputeHash(new
            {
                snapshot.Event.Status,
                snapshot.Summary,
                snapshot.Clans
            }),
            ClanRanks = clanRanks,
            ValidatedFeedIds = validatedFeedIds
        };
    }

    private static string ComputeHash<T>(T value)
    {
        return JsonSerializer.Serialize(value, JsonOptions);
    }

    private sealed class EventRealtimeState
    {
        public string EventStatus { get; init; } = string.Empty;
        public string SnapshotHash { get; init; } = string.Empty;
        public string LeaderboardHash { get; init; } = string.Empty;
        public Dictionary<int, int> ClanRanks { get; init; } = new();
        public HashSet<string> ValidatedFeedIds { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    }
}