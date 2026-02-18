using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("logs")]
public class Logs
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("Message")]
    public string? Message { get; set; }

    [Column("action")]
    public string? Action { get; set; }

    [Column("entity_name")]
    public string? EntityName { get; set; }

    [Column("entity_id")]
    public int? EntityId { get; set; }

    [Column("LogDate")]
    public DateTime LogDate { get; set; }
}