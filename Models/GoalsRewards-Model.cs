using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("goals_rewards")]
public class GoalReward
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("goal_id")]
    public int GoalId { get; set; }

    [Column("badge_id")]
    public int BadgeId { get; set; }

    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("points")]
    public double? Points { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // 🔗 Relacionamentos
    [ForeignKey(nameof(GoalId))]
    public Goal? Goal { get; set; }

    [ForeignKey(nameof(BadgeId))]
    public Badge? Badge { get; set; }
}