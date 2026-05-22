using API_PortalSantosTech.Models.DTO;

namespace API_PortalSantosTech.Interfaces;

public interface IFinalChallengeRealtimeNotifier
{
    Task PublishSnapshotUpdatedAsync(FinalChallengeSnapshotUpdatedEvent payload, CancellationToken cancellationToken = default);
    Task PublishLeaderboardUpdatedAsync(FinalChallengeLeaderboardUpdatedEvent payload, CancellationToken cancellationToken = default);
    Task PublishClanMovedAsync(FinalChallengeClanMovedEvent payload, CancellationToken cancellationToken = default);
    Task PublishChallengeValidatedAsync(FinalChallengeChallengeValidatedEvent payload, CancellationToken cancellationToken = default);
    Task PublishWeekClosedAsync(FinalChallengeWeekClosedEvent payload, CancellationToken cancellationToken = default);
}