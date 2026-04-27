namespace API_PortalSantosTech.Models.DTO;

public class PointRankingDTO
{
    public int UserId { get; set; }
    public float TotalPoints { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
}

public class RankingPerCategoryDTO
{
    public string Category { get; set; } = string.Empty;
    public IEnumerable<CategoryRankingDTO> Rankings { get; set; } = new List<CategoryRankingDTO>();
}

public class CategoryRankingDTO
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public float PercentAvailable { get; set; }
    public int TotalAnswers { get; set; }
    public string? Status => TotalAnswers == 0 ? "Não Iniciado" : (TotalAnswers >= 10 ? "Desbloqueado" : "Em Progresso");
}

public class EventRankingDTO
{
    public string EventName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime => StartTime.AddMinutes(DurationMinutes);
    public List<EventRankingAwardDTO> EventRankingAwards { get; set; } = new List<EventRankingAwardDTO>();
}

public class EventRankingAwardDTO
{
    public string AwardName { get; set; } = string.Empty;
    public int AwardPositionRanking { get; set; }
    public string AwardDescription { get; set; } = string.Empty;
    public string AwardPictureUrl { get; set; } = string.Empty;
}
