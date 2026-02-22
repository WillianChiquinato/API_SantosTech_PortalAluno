namespace API_PortalSantosTech.Models.DTO;

public class QuestionOptionsDTO
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string? Question { get; set; }
    public bool? CorrectAnswer { get; set; }
}