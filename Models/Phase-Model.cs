using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("phase")]
public class Phase
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("module_id")]
    public int ModuleId { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("week_number")]
    public int WeekNumber { get; set; }

    [Column("index_order")]
    public int IndexOrder { get; set; }

    [Column("admin_authorize")]
    public bool AdminAuthorize { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    // 🔗 Relacionamento
    [ForeignKey(nameof(ModuleId))]
    public Module? Module { get; set; }
}