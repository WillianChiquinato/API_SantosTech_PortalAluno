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

public class RankingCategoryDTO
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
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
    public int Id { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime => StartTime.AddMinutes(DurationMinutes);
    public List<EventRankingAwardDTO> EventRankingAwards { get; set; } = new List<EventRankingAwardDTO>();
}

public class EventRankingAwardDTO
{
    public int Id { get; set; }
    public string AwardName { get; set; } = string.Empty;
    public int AwardPositionRanking { get; set; }
    public string AwardDescription { get; set; } = string.Empty;
    public string AwardPictureUrl { get; set; } = string.Empty;
}

public class RankingEventUpsertDTO
{
    public string EventName { get; set; } = string.Empty;
    public int EventType { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime StartTime { get; set; }
    public List<RankingAwardUpsertDTO> Awards { get; set; } = new List<RankingAwardUpsertDTO>();
}

public class RankingAwardUpsertDTO
{
    public string AwardName { get; set; } = string.Empty;
    public int AwardPositionRanking { get; set; }
    public string AwardDescription { get; set; } = string.Empty;
    public string AwardPictureUrl { get; set; } = string.Empty;
}

public class RankingEventDTO
{
    public int Id { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int EventType { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime => StartTime.AddMinutes(DurationMinutes);
    public List<RankingAwardDTO> Awards { get; set; } = new List<RankingAwardDTO>();
}

public class RankingAwardDTO
{
    public int Id { get; set; }
    public string AwardName { get; set; } = string.Empty;
    public int AwardPositionRanking { get; set; }
    public string AwardDescription { get; set; } = string.Empty;
    public string AwardPictureUrl { get; set; } = string.Empty;
}

public class RankingEventHistoryDTO
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserProfilePictureUrl { get; set; }
    public int AwardId { get; set; }
    public string AwardName { get; set; } = string.Empty;
    public string AwardDescription { get; set; } = string.Empty;
    public string AwardPictureUrl { get; set; } = string.Empty;
    public int RankingPosition { get; set; }
    public DateTime RecordedAt { get; set; }
}
