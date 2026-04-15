namespace API_PortalSantosTech.Models.DTO;

public class GoalWithBadgesResponse
{
    public int GoalId { get; set; }
    public int GoalRewardId { get; set; }
    public string GoalName { get; set; } = string.Empty;
    public string GoalDescription { get; set; } = string.Empty;
    public GoalType GoalType { get; set; }
    public string GoalImageUrl { get; set; } = string.Empty;
    public List<BadgeDTO> Badges { get; set; } = new List<BadgeDTO>();
    public double Points { get; set; }
}

public class ActivatedGoalResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int GoalRewardId { get; set; }
    public string GoalName { get; set; } = string.Empty;
    public string GoalDescription { get; set; } = string.Empty;
    public GoalType GoalType { get; set; }
    public RewardType RewardType { get; set; }
    public int CourseId { get; set; }
    public double Progress { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool RewardClaimed { get; set; }
    public DateTime? RewardClaimedAt { get; set; }
}

public class GoalsRewardDTO
{
    public int GoalRewardId { get; set; }
    public int UserId { get; set; }
    public List<BadgeDTO> BadgesReward { get; set; } = new List<BadgeDTO>();
    public double PointsReward { get; set; }
}