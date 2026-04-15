using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("notification_dispatch")]
public class NotificationDispatch
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("notification_template_id")]
    public int NotificationTemplateId { get; set; }

    [Column("template_name")]
    public string TemplateName { get; set; } = string.Empty;

    [Column("triggered_by_actor_id")]
    public string? TriggeredByActorId { get; set; }

    [Column("triggered_by_actor_name")]
    public string? TriggeredByActorName { get; set; }

    [Column("triggered_by_actor_email")]
    public string? TriggeredByActorEmail { get; set; }

    [Column("filters_json")]
    public string FiltersJson { get; set; } = string.Empty;

    [Column("total_recipients")]
    public int TotalRecipients { get; set; }

    [Column("failed_recipients")]
    public int FailedRecipients { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(NotificationTemplateId))]
    public NotificationTemplate? NotificationTemplate { get; set; }
}
