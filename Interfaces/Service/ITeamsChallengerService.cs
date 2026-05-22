using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface ITeamsChallengerService
{
    Task<CustomResponse<FinalChallengeSnapshot>> GetLiveSnapshotAsync(int eventId, int? currentUserId);
    Task<CustomResponse<IEnumerable<FinalChallengeClan>>> GetLeaderboardAsync(int eventId, int? currentUserId);
    Task<CustomResponse<IEnumerable<FinalChallengeTask>>> GetClanTasksAsync(int eventId, int clanId);
    Task<CustomResponse<IEnumerable<FinalChallengeFeedItem>>> GetActivityFeedAsync(int eventId);
    Task<CustomResponse<FinalChallengeSubmitResult>> SubmitAnswerAsync(FinalChallengeAnswerRequest answerRequest, int? currentUserId);
    Task<CustomResponse<TeamsUserChallengerDTO>> GetTeamForPlayerRelationship(int classId, int moduleId, int? currentUserId);
    Task<CustomResponse<CreateTeamRequest>> CreateTeamAsync(CreateTeamRequest createTeamRequest, int? currentUserId);
}
