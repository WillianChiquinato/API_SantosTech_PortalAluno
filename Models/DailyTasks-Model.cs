using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("daily_tasks")]
public class DailyTask
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("exercise_id")]
    public int ExerciseId { get; set; }

    [Column("phase_id")]
    public int PhaseId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // 🔗 Relacionamentos
    [ForeignKey(nameof(ExerciseId))]
    public Exercise? Exercise { get; set; }

    [ForeignKey(nameof(PhaseId))]
    public Phase? Phase { get; set; }
}