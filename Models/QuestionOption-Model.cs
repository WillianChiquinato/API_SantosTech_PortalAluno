using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("question_option")]
public class QuestionOption
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("question_id")]
    public int QuestionId { get; set; }

    [Column("option_text")]
    public string? OptionText { get; set; }

    [Column("is_correct")]
    public bool IsCorrect { get; set; }

    // 🔗 Relacionamento
    [ForeignKey(nameof(QuestionId))]
    public Question? Question { get; set; }
}