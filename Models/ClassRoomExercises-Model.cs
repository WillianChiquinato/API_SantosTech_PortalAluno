using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("class_room_exercises")]
public class ClassRoomExercise
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("class_room_id")]
    public int ClassRoomId { get; set; }

    [Column("exercise_id")]
    public int ExerciseId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // 🔗 Relacionamentos
    [ForeignKey(nameof(ClassRoomId))]
    public ClassRoom? ClassRoom { get; set; }

    [ForeignKey(nameof(ExerciseId))]
    public Exercise? Exercise { get; set; }
}