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
