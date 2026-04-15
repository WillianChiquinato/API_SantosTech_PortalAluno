using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("goals_students")]
public class GoalStudent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("goal_reward_id")]
    public int GoalRewardId { get; set; }

    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("progress")]
    public double Progress { get; set; }

    [Column("is_completed")]
    public bool IsCompleted { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [Column("reward_claimed")]
    public bool RewardClaimed { get; set; }

    [Column("reward_claimed_at")]
    public DateTime? RewardClaimedAt { get; set; }

    // 🔗 Relacionamentos
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(GoalRewardId))]
    public GoalReward? GoalReward { get; set; }
}