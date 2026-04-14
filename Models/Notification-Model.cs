using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Models;

[Table("notification")]
[Index(nameof(UserId), nameof(ReadAt))]
public class Notification
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("notification_template_id")]
    public int NotificationTemplateId { get; set; }

    [Column("notification_dispatch_id")]
    public int NotificationDispatchId { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("metadata_json")]
    public string MetadataJson { get; set; } = string.Empty;

    [Column("read_at")]
    public DateTime? ReadAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(NotificationTemplateId))]
    public NotificationTemplate? NotificationTemplate { get; set; }

    [ForeignKey(nameof(NotificationDispatchId))]
    public NotificationDispatch? NotificationDispatch { get; set; }
}
