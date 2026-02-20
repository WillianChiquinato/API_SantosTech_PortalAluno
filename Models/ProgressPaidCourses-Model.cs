using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("progress_paid_courses")]
public class ProgressPaidCourses
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("progress_percentage")]
    public decimal ProgressPercentage { get; set; }

    [Column("last_accessed")]
    public DateTime LastAccessed { get; set; }

    // 🔗 Relacionamento
    [ForeignKey("UserId")]
    public User? User { get; set; }

    [ForeignKey("CourseId")]
    public Course? Course { get; set; }
}