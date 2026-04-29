using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

public enum EventType
{
    Notas = 1,
    Pontos = 2,
    Outro = 3
}

[Table("ranking_events")]
public class RankingEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("event_name")]
    public string EventName { get; set; } = string.Empty;

    [Column("event_type")]
    public EventType EventType { get; set; }

    [Column("duration_minutes")]
    public int DurationMinutes { get; set; }

    [Column("start_time", TypeName = "timestamp with time zone")]
    public DateTime StartTime { get; set; }

    [Column("scheduled_job_id")]
    public string? ScheduledJobId { get; set; }
}
