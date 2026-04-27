using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("ranking_awards")]
public class RankingAward
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("award_name")]
    public string AwardName { get; set; } = string.Empty;

    [Column("award_position_ranking")]
    public int AwardPositionRanking { get; set; }

    [Column("award_description")]
    public string AwardDescription { get; set; } = string.Empty;

    [Column("award_picture_url")]
    public string AwardPictureUrl { get; set; } = string.Empty;

    // 🔗 Relacionamento
    [ForeignKey(nameof(EventId))]
    public RankingEvent Event { get; set; }
}