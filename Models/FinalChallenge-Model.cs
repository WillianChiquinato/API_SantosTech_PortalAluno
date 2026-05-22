using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("final_challenge_event")]
public class FinalChallengeEventRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("ranking_event_id")]
    public int? RankingEventId { get; set; }

    [Column("module_id")]
    public int ModuleId { get; set; }

    [Column("class_id")]
    public int ClassId { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("status")]
    public string Status { get; set; } = "scheduled";

    [Column("starts_at", TypeName = "timestamp with time zone")]
    public DateTime StartsAt { get; set; }

    [Column("ends_at", TypeName = "timestamp with time zone")]
    public DateTime EndsAt { get; set; }

    [Column("refresh_interval_seconds")]
    public int RefreshIntervalSeconds { get; set; } = 10;

    [Column("update_mode")]
    public string UpdateMode { get; set; } = "signalr";

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp with time zone")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey(nameof(RankingEventId))]
    public RankingEvent? RankingEvent { get; set; }

    [ForeignKey(nameof(ModuleId))]
    public Module? Module { get; set; }

    [ForeignKey(nameof(ClassId))]
    public Class? Class { get; set; }
}

[Table("final_challenge_clan")]
public class FinalChallengeClanRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("team_id")]
    public int? TeamId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("motto")]
    public string Motto { get; set; } = string.Empty;

    [Column("color")]
    public string Color { get; set; } = string.Empty;

    [Column("boat_name")]
    public string BoatName { get; set; } = string.Empty;

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp with time zone")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey(nameof(EventId))]
    public FinalChallengeEventRecord? Event { get; set; }

    [ForeignKey(nameof(TeamId))]
    public TeamsChallenger? Team { get; set; }
}

[Table("final_challenge_task")]
public class FinalChallengeTaskRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("exercise_id")]
    public int? ExerciseId { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("category")]
    public string Category { get; set; } = string.Empty;

    [Column("briefing")]
    public string? Briefing { get; set; }

    [Column("difficulty")]
    public string Difficulty { get; set; } = "easy";

    [Column("score_weight")]
    public int ScoreWeight { get; set; }

    [Column("deadline_at", TypeName = "timestamp with time zone")]
    public DateTime DeadlineAt { get; set; }

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp with time zone")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey(nameof(EventId))]
    public FinalChallengeEventRecord? Event { get; set; }

    [ForeignKey(nameof(ExerciseId))]
    public Exercise? Exercise { get; set; }
}

[Table("final_challenge_submission")]
public class FinalChallengeSubmissionRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("clan_id")]
    public int ClanId { get; set; }

    [Column("task_id")]
    public int TaskId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("answer")]
    public string Answer { get; set; } = string.Empty;

    [Column("attachments_json")]
    public string? AttachmentsJson { get; set; }

    [Column("status")]
    public string Status { get; set; } = "queued";

    [Column("ai_score")]
    public double? AiScore { get; set; }

    [Column("submitted_at", TypeName = "timestamp with time zone")]
    public DateTime SubmittedAt { get; set; }

    [Column("validated_at", TypeName = "timestamp with time zone")]
    public DateTime? ValidatedAt { get; set; }

    [ForeignKey(nameof(EventId))]
    public FinalChallengeEventRecord? Event { get; set; }

    [ForeignKey(nameof(ClanId))]
    public FinalChallengeClanRecord? Clan { get; set; }

    [ForeignKey(nameof(TaskId))]
    public FinalChallengeTaskRecord? Task { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}

[Table("final_challenge_activity")]
public class FinalChallengeActivityRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("clan_id")]
    public int? ClanId { get; set; }

    [Column("type")]
    public string Type { get; set; } = string.Empty;

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("happened_at", TypeName = "timestamp with time zone")]
    public DateTime HappenedAt { get; set; }

    [Column("metadata_json")]
    public string? MetadataJson { get; set; }

    [ForeignKey(nameof(EventId))]
    public FinalChallengeEventRecord? Event { get; set; }

    [ForeignKey(nameof(ClanId))]
    public FinalChallengeClanRecord? Clan { get; set; }
}