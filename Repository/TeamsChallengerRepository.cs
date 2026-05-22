using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class TeamsChallengerRepository : ITeamsChallengerRepository
{
    private readonly AppDbContext _efDbContext;

    public TeamsChallengerRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<CreateTeamRequest> CreateTeamAsync(CreateTeamRequest createTeamRequest, int userId)
    {
        var team = new TeamsChallenger
        {
            Name = createTeamRequest.Name,
            Description = createTeamRequest.Description,
            ClassId = createTeamRequest.ClassId,
            ModuleId = createTeamRequest.ModuleId,
            ClanColor = createTeamRequest.ClanColor,
            BoatName = createTeamRequest.BoatName,
            CreatedAt = DateTime.UtcNow
        };

        _efDbContext.TeamsChallengers.Add(team);
        await _efDbContext.SaveChangesAsync();

        var teamUser = new TeamUsersChallenger
        {
            TeamId = team.Id,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _efDbContext.TeamUsersChallengers.Add(teamUser);
        await _efDbContext.SaveChangesAsync();

        return new CreateTeamRequest
        {
            ClassId = team.ClassId,
            ModuleId = team.ModuleId,
            Name = team.Name,
            Description = team.Description,
            ClanColor = team.ClanColor,
            BoatName = team.BoatName,
            UserIds = createTeamRequest.UserIds
        };
    }

    public async Task<TeamsChallenger?> GetTeamForPlayerRelationshipAsync(int classId, int moduleId, int userId)
    {
        var teams = await _efDbContext.TeamsChallengers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ClassId == classId && x.ModuleId == moduleId);

        return teams;
    }

    public async Task<List<TeamUsersChallenger>> GetUsersOfTeamAsync(int teamId)
    {
        return await _efDbContext.TeamUsersChallengers
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.TeamId == teamId)
            .ToListAsync();
    }
}
