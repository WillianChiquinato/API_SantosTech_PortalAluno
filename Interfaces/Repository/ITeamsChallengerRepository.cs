using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface ITeamsChallengerRepository
{
    Task<TeamsChallenger?> GetTeamForPlayerRelationshipAsync(int classId, int moduleId, int userId);
    Task<List<TeamUsersChallenger>> GetUsersOfTeamAsync(int teamId);
    Task<CreateTeamRequest> CreateTeamAsync(CreateTeamRequest createTeamRequest, int userId);
}
