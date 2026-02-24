namespace API_PortalSantosTech.Models.DTO;

public class AddPointsDTO
{
    public int UserId { get; set; }
    public int PointsToAdd { get; set; }
    public DateTime ExerciseDate { get; set; }
}