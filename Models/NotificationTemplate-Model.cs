using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("notification_template")]
public class NotificationTemplate
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("title_template")]
    public string TitleTemplate { get; set; } = string.Empty;

    [Column("message_template")]
    public string MessageTemplate { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("created_by_actor_id")]
    public string? CreatedByActorId { get; set; }

    [Column("created_by_actor_name")]
    public string? CreatedByActorName { get; set; }

    [Column("created_by_actor_email")]
    public string? CreatedByActorEmail { get; set; }

    [Column("updated_by_actor_id")]
    public string? UpdatedByActorId { get; set; }

    [Column("updated_by_actor_name")]
    public string? UpdatedByActorName { get; set; }

    [Column("updated_by_actor_email")]
    public string? UpdatedByActorEmail { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
