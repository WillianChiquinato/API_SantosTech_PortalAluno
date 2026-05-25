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

    public async Task<FinalChallengeEventRecord?> GetCurrentActivityEventAsync(int userId)
    {
        var currentEvent = await _efDbContext.FinalChallengeEvents
            .AsNoTracking()
            .Where(x => x.StartsAt.Date <= DateTime.UtcNow.Date && x.EndsAt.Date >= DateTime.UtcNow.Date)
            .FirstOrDefaultAsync();

        return currentEvent;
    }

    public async Task<List<RankingFinalChallengeDTO>> GetRankingToFinalChallengeAsync(int eventId)
    {
        var clans = await _efDbContext.FinalChallengeClans
            .AsNoTracking()
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.Id)
            .ToListAsync();

        if (clans.Count == 0)
            return new List<RankingFinalChallengeDTO>();

        var tasks = await _efDbContext.FinalChallengeTasks
            .AsNoTracking()
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.DeadlineAt)
            .ThenBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.ScoreWeight
            })
            .ToListAsync();

        var teamIds = clans
            .Where(x => x.TeamId.HasValue)
            .Select(x => x.TeamId!.Value)
            .Distinct()
            .ToList();

        var membersCountByTeamId = teamIds.Count == 0
            ? new Dictionary<int, int>()
            : await _efDbContext.TeamUsersChallengers
                .AsNoTracking()
                .Where(x => teamIds.Contains(x.TeamId))
                .GroupBy(x => x.TeamId)
                .Select(group => new
                {
                    TeamId = group.Key,
                    MembersCount = group.Select(x => x.UserId).Distinct().Count()
                })
                .ToDictionaryAsync(x => x.TeamId, x => x.MembersCount);

        var validatedSubmissions = await _efDbContext.FinalChallengeSubmissions
            .AsNoTracking()
            .Where(x => x.EventId == eventId && x.ValidatedAt != null)
            .Join(
                _efDbContext.FinalChallengeTasks.AsNoTracking().Where(x => x.EventId == eventId),
                submission => submission.TaskId,
                task => task.Id,
                (submission, task) => new
                {
                    submission.ClanId,
                    submission.TaskId,
                    AiScore = submission.AiScore ?? 0d,
                    submission.SubmittedAt,
                    ValidatedAt = submission.ValidatedAt ?? submission.SubmittedAt,
                    task.ScoreWeight
                })
            .ToListAsync();

        var bestSubmissionByClanAndTask = validatedSubmissions
            .GroupBy(x => new { x.ClanId, x.TaskId })
            .Select(group => group
                .OrderByDescending(x => x.AiScore)
                .ThenBy(x => x.ValidatedAt)
                .First())
            .ToList();

        var now = DateTime.UtcNow;
        var totalTasks = tasks.Count;

        var ranking = clans
            .Select(clan =>
            {
                var clanSubmissions = bestSubmissionByClanAndTask
                    .Where(x => x.ClanId == clan.Id)
                    .ToList();

                var solvedTaskIds = clanSubmissions
                    .Select(x => x.TaskId)
                    .Distinct()
                    .ToHashSet();

                var membersCount = clan.TeamId.HasValue && membersCountByTeamId.TryGetValue(clan.TeamId.Value, out var count)
                    ? count
                    : 0;

                var averageAiScore = clanSubmissions.Count == 0
                    ? 0
                    : clanSubmissions.Average(x => x.AiScore);

                var averageResolutionSeconds = clanSubmissions.Count == 0
                    ? 0
                    : clanSubmissions.Average(x => Math.Max(0, (x.ValidatedAt - x.SubmittedAt).TotalSeconds));

                var totalScore = clanSubmissions.Sum(x => x.AiScore * Math.Max(1, x.ScoreWeight));
                var solvedCount = solvedTaskIds.Count;
                var progressPercent = totalTasks == 0 ? 0 : solvedCount * 100d / totalTasks;
                var nextTask = tasks.FirstOrDefault(x => !solvedTaskIds.Contains(x.Id));
                var streak = clanSubmissions
                    .Where(x => x.ValidatedAt >= now.AddHours(-1))
                    .Select(x => x.TaskId)
                    .Distinct()
                    .Count();

                return new RankingFinalChallengeDTO
                {
                    ClanId = clan.Id,
                    ClanName = clan.Name,
                    Motto = clan.Motto,
                    Color = clan.Color,
                    BoatName = clan.BoatName,
                    MembersCount = membersCount,
                    SolvedCount = solvedCount,
                    TotalScore = Math.Round(totalScore, 2),
                    AverageAiScore = Math.Round(averageAiScore, 2),
                    AverageResolutionSeconds = Math.Round(averageResolutionSeconds, 2),
                    ProgressPercent = Math.Round(progressPercent, 2),
                    DistanceToFinishPercent = Math.Round(Math.Max(0, 100 - progressPercent), 2),
                    CurrentCheckpoint = nextTask?.Title ?? "Concluido",
                    Rank = 0
                };
            })
            .OrderByDescending(x => x.TotalScore)
            .ThenByDescending(x => x.SolvedCount)
            .ThenBy(x => x.ClanName)
            .ToList();

        for (var index = 0; index < ranking.Count; index++)
        {
            ranking[index].Rank = index + 1;
        }

        return ranking;
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
