using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("ranking_history")]
public class RankingHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("award_id")]
    public int AwardId { get; set; }

    [Column("ranking_position")]
    public int RankingPosition { get; set; }

    [Column("recorded_at", TypeName = "timestamp")]
    public DateTime RecordedAt { get; set; }

    // 🔗 Relacionamentos
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    [ForeignKey(nameof(AwardId))]
    public RankingAward Award { get; set; }

    [ForeignKey(nameof(EventId))]
    public RankingEvent Event { get; set; }
}