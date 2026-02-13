using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("class")]
public class Class
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("current_module_id")]
    public int CurrentModuleId { get; set; }

    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // 🔗 Relacionamentos
    [ForeignKey(nameof(CurrentModuleId))]
    public Module? CurrentModule { get; set; }

    [ForeignKey(nameof(CourseId))]
    public Course? Course { get; set; }
}