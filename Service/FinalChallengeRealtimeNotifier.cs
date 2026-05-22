using API_PortalSantosTech.Hubs;
using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Models.DTO;
using Microsoft.AspNetCore.SignalR;

namespace API_PortalSantosTech.Services;

public class FinalChallengeRealtimeNotifier : IFinalChallengeRealtimeNotifier
{
    private readonly IHubContext<FinalChallengeHub> _hubContext;

    public FinalChallengeRealtimeNotifier(IHubContext<FinalChallengeHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublishSnapshotUpdatedAsync(FinalChallengeSnapshotUpdatedEvent payload, CancellationToken cancellationToken = default)
    {
        return BroadcastAsync(payload.EventId, "snapshotUpdated", payload, cancellationToken);
    }

    public Task PublishLeaderboardUpdatedAsync(FinalChallengeLeaderboardUpdatedEvent payload, CancellationToken cancellationToken = default)
    {
        return BroadcastAsync(payload.EventId, "leaderboardUpdated", payload, cancellationToken);
    }

    public Task PublishClanMovedAsync(FinalChallengeClanMovedEvent payload, CancellationToken cancellationToken = default)
    {
        return BroadcastAsync(payload.EventId, "clanMoved", payload, cancellationToken);
    }

    public Task PublishChallengeValidatedAsync(FinalChallengeChallengeValidatedEvent payload, CancellationToken cancellationToken = default)
    {
        return BroadcastAsync(payload.EventId, "challengeValidated", payload, cancellationToken);
    }

    public Task PublishWeekClosedAsync(FinalChallengeWeekClosedEvent payload, CancellationToken cancellationToken = default)
    {
        return BroadcastAsync(payload.EventId, "weekClosed", payload, cancellationToken);
    }

    private Task BroadcastAsync<TPayload>(int eventId, string eventName, TPayload payload, CancellationToken cancellationToken)
    {
        return _hubContext.Clients
            .Group(FinalChallengeHub.BuildGroupName(eventId))
            .SendAsync(eventName, payload, cancellationToken);
    }
}