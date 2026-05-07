using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("materials_reference_exercises")]
public class MaterialsReferenceExercises
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("material_id")]
    public int MaterialId { get; set; }

    [Column("exercise_id")]
    public int ExerciseId { get; set; }

    [Column("reference_description")]
    public string ReferenceDescription { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // 🔗 Relacionamentos
    [ForeignKey(nameof(MaterialId))]
    public Material? Material { get; set; }

    [ForeignKey(nameof(ExerciseId))]
    public Exercise? Exercise { get; set; }
}