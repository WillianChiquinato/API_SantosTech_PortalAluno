using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("course")]
public class Course
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_paid")]
    public bool IsPaid { get; set; }

    [Column("price")]
    public decimal Price { get; set; }

    [Column("duration_hours")]
    public int Duration { get; set; }

    [Column("level_difficulty")]
    public string? LevelDifficulty { get; set; }

    [Column("paid_focus")]
    public string? PaidFocus { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}