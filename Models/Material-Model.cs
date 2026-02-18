using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

public enum MaterialVisiblity
{
    Private = 0,
    Public = 1,
    Alunos = 2
}

[Table("material")]
public class Material
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("class_id")]
    public int ClassId { get; set; }

    [Column("title")]
    public string? Title { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("file_url")]
    public string? FileUrl { get; set; }

    [Column("file_type")]
    public string? FileType { get; set; }

    [Column("visibility")]
    public MaterialVisiblity Visibility { get; set; }

    [Column("uploaded_at")]
    public DateTime UploadedAt { get; set; }

    // 🔗 Relacionamento
    [ForeignKey(nameof(ClassId))]
    public Class? Class { get; set; }
}