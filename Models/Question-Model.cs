using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("question")]
public class Question
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("statement")]
    public string? Statement { get; set; }

    [Column("exercise_id")]
    public int ExerciseId { get; set; }

    // 🔗 Relacionamento
    [ForeignKey(nameof(ExerciseId))]
    public Exercise? Exercise { get; set; }
}