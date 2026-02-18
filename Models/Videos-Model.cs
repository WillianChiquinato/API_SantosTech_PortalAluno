using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

public enum Visibility
{
    Professor = 1,
    Aluno = 2
}

[Table("video")]
public class Video
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("title")]
    public string? Title { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("url")]
    public string? Url { get; set; }

    [Column("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    [Column("duration_seconds")]
    public int DurationSeconds { get; set; }

    [Column("visibility")]
    public Visibility Visibility { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}