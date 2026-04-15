using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

public enum RewardType
{
    PointsBasic = 1,
    PointsBetweenDates = 2,
}

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
    public double? PointsReward { get; set; }

    [Column("points_target")]
    public double? PointsTarget { get; set; }

    [Column("reward_type")]
    public RewardType RewardType { get; set; }

    [Column("start_date_target")]
    public DateTime? StartDateTarget { get; set; }
    
    [Column("end_date_target")]
    public DateTime? EndDateTarget { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // 🔗 Relacionamentos
    [ForeignKey(nameof(GoalId))]
    public Goal? Goal { get; set; }

    [ForeignKey(nameof(BadgeId))]
    public Badge? Badge { get; set; }

    [ForeignKey(nameof(CourseId))]
    public Course? Course { get; set; }
}