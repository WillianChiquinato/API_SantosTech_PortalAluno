using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("class_rooms")]
public class ClassRoom
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("class_id")]
    public int ClassId { get; set; }

    [Column("name")]
    [Required]
    public string Name { get; set; }

    [Column("is_authorized")]
    public bool IsAuthorized { get; set; }

    [Column("target_limited")]
    public DateTime? TargetLimited { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // 🔗 Relacionamento
    [ForeignKey(nameof(ClassId))]
    public Class? Class { get; set; }
}