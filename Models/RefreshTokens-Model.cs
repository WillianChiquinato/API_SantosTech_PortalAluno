using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("refresh_tokens")]
public class RefreshToken
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Required]
    [Column("token_hash")]
    public string TokenHash { get; set; }

    [Required]
    [Column("expires_at", TypeName = "timestamp")]
    public DateTime ExpiresAt { get; set; }

    [Column("revoked_at", TypeName = "timestamp")]
    public DateTime? RevokedAt { get; set; }

    [Required]
    [Column("created_at", TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; }

    // 🔗 Relacionamento  
    [ForeignKey("UserId")]
    public User User { get; set; }
}