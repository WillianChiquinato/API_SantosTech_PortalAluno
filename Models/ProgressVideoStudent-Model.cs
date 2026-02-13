using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("progress_video_student")]
public class ProgressVideoStudent
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("video_id")]
    public int VideoId { get; set; }

    [Column("watched_seconds")]
    public int WatchedSeconds { get; set; }

    [Column("is_completed")]
    public bool IsCompleted { get; set; }

    [Column("last_watched_at")]
    public DateTime? LastWatchedAt { get; set; }

    // 🔗 Relacionamentos
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(VideoId))]
    public Video? Video { get; set; }
}