namespace API_PortalSantosTech.Models.DTO;

public class VideoProgressDTO
{
    public int VideoId { get; set; }
    public int UserId { get; set; }
    public int WatchSeconds { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime LastWatched { get; set; }
}