using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

public enum FlowOrigin
{
    Main = 1,
    Lower = 2
}


[Table("user_exercise_flow")]
public class UserExerciseFlow
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("phase_id")]
    public int PhaseId { get; set; }

    [Column("exercise_id")]
    public int ExerciseId { get; set; }

    [Column("index_order")]
    public int IndexOrder { get; set; }

    [Column("origin")]
    public FlowOrigin Origin { get; set; }

    [Column("triggered_by_exercise_id")]
    public int? TriggeredByExerciseId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // 🔗 Relacionamentos
    [ForeignKey(nameof(ExerciseId))]
    public Exercise? Exercise { get; set; }

    [ForeignKey(nameof(PhaseId))]
    public Phase? Phase { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}