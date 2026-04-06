using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Models;

[Table("user_identity")]
[Index(nameof(UserId), nameof(Provider), IsUnique = true)]
[Index(nameof(Provider), nameof(ProviderUserId), IsUnique = true)]
public class UserIdentity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("provider")]
    public string Provider { get; set; } = string.Empty;

    [Column("provider_user_id")]
    public string ProviderUserId { get; set; } = string.Empty;

    [Column("provider_email")]
    public string ProviderEmail { get; set; } = string.Empty;

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp with time zone")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
