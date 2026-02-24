using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("configs")]
public class Configs
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("receive_email_notifications")]
    public bool ReceiveEmailNotifications { get; set; }

    [Column("dark_mode_enabled")]
    public bool DarkModeEnabled { get; set; }

    [Column("report_frequency")]
    public bool ReportFrequency { get; set; } = false;

    [Column("acessibility_mode")]
    public bool AcessibilityMode { get; set; } = false;

    [Column("preferred_language")]
    public string PreferredLanguage { get; set; } = string.Empty;

    // 🔗 Relacionamentos
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}