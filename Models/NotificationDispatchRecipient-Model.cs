using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("notification_dispatch_recipient")]
public class NotificationDispatchRecipient
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("notification_dispatch_id")]
    public int NotificationDispatchId { get; set; }

    [Column("recipient_user_id")]
    public int? RecipientUserId { get; set; }

    [Column("recipient_name")]
    public string? RecipientName { get; set; }

    [Column("recipient_email")]
    public string? RecipientEmail { get; set; }

    [Column("class_name")]
    public string? ClassName { get; set; }

    [Column("course_name")]
    public string? CourseName { get; set; }

    [Column("title")]
    public string? Title { get; set; }

    [Column("message")]
    public string? Message { get; set; }

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column("failure_reason")]
    public string? FailureReason { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(NotificationDispatchId))]
    public NotificationDispatch? NotificationDispatch { get; set; }
}
