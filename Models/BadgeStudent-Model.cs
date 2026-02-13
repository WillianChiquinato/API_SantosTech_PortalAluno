using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("badge_student")]
public class BadgeStudent
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("badge_id")]
    public int BadgeId { get; set; }

    [Column("awarded_at")]
    public DateTime AwardedAt { get; set; }

    // 🔗 Relacionamentos
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(BadgeId))]
    public Badge? Badge { get; set; }
}