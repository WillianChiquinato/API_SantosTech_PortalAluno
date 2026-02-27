using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

public enum StatusStates
{
    Pending = 0,
    Unlocked = 1,
    Completed = 2
}

[Table("progress_student_phase")]
public class ProgressStudentPhase
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("phase_id")]
    public int PhaseId { get; set; }

    [Column("progress")]
    public double Progress { get; set; }

    [Column("status")]
    public StatusStates Status { get; set; } = StatusStates.Pending;

    [Column("unlocked_at")]
    public DateTime? UnlockedAt { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // 🔗 Relacionamentos
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(PhaseId))]
    public Phase? Phase { get; set; }
}