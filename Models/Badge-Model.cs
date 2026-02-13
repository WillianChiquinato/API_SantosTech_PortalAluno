using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("badge")]
public class Badge
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("description")]
    public string Description { get; set; } = null!;

    [Column("icon_url")]
    public string IconUrl { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}