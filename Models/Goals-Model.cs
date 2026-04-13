using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

public enum GoalType
{
    CourseCompletion = 1,
    PhaseCompletion = 2,
    TaskQuantity = 3,
    TimeSpent = 4,
    Custom = 5
}

[Table("goals")]
public class Goal
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("type")]
    public GoalType? Type { get; set; }

    [Column("image_url")]
    public string? ImageUrl { get; set; }
}