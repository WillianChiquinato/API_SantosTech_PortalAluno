using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("teams_users_challenger")]
public class TeamUsersChallenger
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("team_id")]
    public int TeamId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // 🔗 Relacionamentos
    [ForeignKey(nameof(TeamId))]
    public TeamsChallenger? Team { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}