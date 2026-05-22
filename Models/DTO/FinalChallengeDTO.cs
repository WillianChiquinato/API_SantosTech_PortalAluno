namespace API_PortalSantosTech.Models.DTO;

public sealed class FinalChallengeEventWindow
{
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string Status { get; set; } = "scheduled";
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public DateTime ServerTime { get; set; }
    public int RefreshIntervalSeconds { get; set; }
    public string UpdateMode { get; set; } = "polling";
}

public sealed class FinalChallengeClan
{
    public int ClanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Motto { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string BoatName { get; set; } = string.Empty;
    public int MembersCount { get; set; }
    public int SolvedCount { get; set; }
    public double TotalScore { get; set; }
    public double AverageAiScore { get; set; }
    public double AverageResolutionSeconds { get; set; }
    public double ProgressPercent { get; set; }
    public double DistanceToFinishPercent { get; set; }
    public string CurrentCheckpoint { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public bool IsCurrentUserClan { get; set; }
    public int Streak { get; set; }
}

public sealed class FinalChallengeTask
{
    public int ChallengeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Briefing { get; set; }
    public string Difficulty { get; set; } = "easy";
    public string Status { get; set; } = "locked";
    public int ScoreWeight { get; set; }
    public DateTime DeadlineAt { get; set; }
    public DateTime? BestSubmissionAt { get; set; }
    public double? AiScore { get; set; }
}

public sealed class FinalChallengeFeedItem
{
    public string Id { get; set; } = string.Empty;
    public int ClanId { get; set; }
    public string ClanName { get; set; } = string.Empty;
    public string Type { get; set; } = "submission";
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime HappenedAt { get; set; }
}

public sealed class FinalChallengeSummary
{
    public int TotalClans { get; set; }
    public int TotalParticipants { get; set; }
    public int TotalChallenges { get; set; }
    public int ValidatedSubmissions { get; set; }
    public int CurrentUserClanRank { get; set; }
    public double CurrentUserClanScore { get; set; }
}

public sealed class FinalChallengeSnapshot
{
    public FinalChallengeEventWindow Event { get; set; } = new();
    public FinalChallengeSummary Summary { get; set; } = new();
    public List<FinalChallengeClan> Clans { get; set; } = new();
    public List<FinalChallengeTask> Tasks { get; set; } = new();
    public List<FinalChallengeFeedItem> Feed { get; set; } = new();
}

public sealed class FinalChallengeAnswerRequest
{
    public int EventId { get; set; }
    public int ChallengeId { get; set; }
    public string Answer { get; set; } = string.Empty;
    public List<string> Attachments { get; set; } = new();
}

public sealed class FinalChallengeSubmitResult
{
    public int SubmissionId { get; set; }
    public string Status { get; set; } = "queued";
    public string Message { get; set; } = string.Empty;
}

public sealed class FinalChallengeSnapshotUpdatedEvent
{
    public int EventId { get; set; }
    public FinalChallengeSnapshot Snapshot { get; set; } = new();
}

public sealed class FinalChallengeLeaderboardUpdatedEvent
{
    public int EventId { get; set; }
    public DateTime ServerTime { get; set; }
    public FinalChallengeSummary Summary { get; set; } = new();
    public List<FinalChallengeClan> Clans { get; set; } = new();
}

public sealed class FinalChallengeClanMovedEvent
{
    public int EventId { get; set; }
    public int ClanId { get; set; }
    public string ClanName { get; set; } = string.Empty;
    public int PreviousRank { get; set; }
    public int CurrentRank { get; set; }
    public DateTime HappenedAt { get; set; }
    public FinalChallengeClan Clan { get; set; } = new();
}

public sealed class FinalChallengeChallengeValidatedEvent
{
    public int EventId { get; set; }
    public DateTime HappenedAt { get; set; }
    public FinalChallengeFeedItem Item { get; set; } = new();
}

public sealed class FinalChallengeWeekClosedEvent
{
    public int EventId { get; set; }
    public DateTime ClosedAt { get; set; }
    public FinalChallengeSummary Summary { get; set; } = new();
    public List<FinalChallengeClan> FinalLeaderboard { get; set; } = new();
}

public sealed class ChallengeContext
{
    public string? Error { get; init; }
    public int EventId { get; init; }
    public int ModuleId { get; init; }
    public FinalChallengeEventWindow? EventWindow { get; init; }
    public List<TeamsChallenger> Teams { get; init; } = new();
    public List<MembersChallenger> Members { get; init; } = new();
    public List<Exercise> Exercises { get; init; } = new();
    public List<Question> Questions { get; init; } = new();
    public List<Answer> Answers { get; init; } = new();
    public List<ProgressExerciseStudent> Progresses { get; init; } = new();
    public List<FinalModuleSubmission> Submissions { get; init; } = new();
    public int? CurrentUserClanId { get; init; }
    public Dictionary<int, TeamMetric> TeamMetrics { get; init; } = new();
    public List<FinalChallengeClan> Clans { get; init; } = new();
}

public sealed class TeamMetric
{
    public required TeamsChallenger Team { get; init; }
    public List<MembersChallenger> Members { get; init; } = new();
    public List<Answer> Answers { get; init; } = new();
    public List<ProgressExerciseStudent> Progresses { get; init; } = new();
    public List<FinalModuleSubmission> Submissions { get; init; } = new();
    public List<int> SolvedExerciseIds { get; init; } = new();
    public double TotalScore { get; init; }
    public double AverageAiScore { get; init; }
    public double AverageResolutionSeconds { get; init; }
    public double ProgressPercent { get; init; }
    public string CurrentCheckpoint { get; init; } = string.Empty;
    public bool IsCurrentUserClan { get; init; }
    public int Streak { get; init; }
}

public class TeamsUserChallengerDTO
{
    public string? ClanName { get; set; }
    public string? ClanDescription { get; set; }
    public string? ClanColor { get; set; }
    public string? BoatName { get; set; }

    public List<TeamsClanMemberDTO> Members { get; set; } = new();
}

public class TeamsClanMemberDTO
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? UserAvatarUrl { get; set; }
    public bool IsCurrentUser { get; set; }
}