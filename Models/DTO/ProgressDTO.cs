namespace API_PortalSantosTech.Models.DTO;

public class ProgressDTO
{
    public int UserId { get; set; }
    public string? Type { get; set; }
    public int Value { get; set; }
}

public class UpdateGoalProgressRequest
{
    public int GoalType { get; set; }
    public int RewardType { get; set; }
}

public class ProgressGoalStudent
{
    public int GoalStudentId { get; set; }
    public int UserId { get; set; }
    public int GoalType { get; set; }
    public int RewardType { get; set; }
    public double ProgressValue { get; set; }
}

public class EvaluateProgressRequest
{
    public int GoalRewardId { get; set; }
    public int RewardType { get; set; }
}