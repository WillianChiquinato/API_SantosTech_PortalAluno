using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("final_module_submission")]
public class FinalModuleSubmission
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("module_id")]
    public int ModuleId { get; set; }

    [Column("team_id")]
    public int TeamId { get; set; }

    [Column("submission_url")]
    public string SubmissionUrl { get; set; } = null!;

    [Column("score")]
    public int Score { get; set; }

    [Column("feedback")]
    public string Feedback { get; set; } = null!;

    [Column("submission_date")]
    public DateTime SubmissionDate { get; set; }

    // 🔗 Relacionamento
    [ForeignKey(nameof(ModuleId))]
    public Module? Module { get; set; }

    [ForeignKey(nameof(TeamId))]
    public TeamsChallenger? Team { get; set; }
}