using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Data;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Services;

public class TeamsChallengerService : ITeamsChallengerService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<TeamsChallengerService> _logger;
    private readonly ITeamsChallengerRepository _teamsChallengerRepository;

    public TeamsChallengerService(
        AppDbContext dbContext,
        ILogger<TeamsChallengerService> logger,
        ITeamsChallengerRepository teamsChallengerRepository)
    {
        _dbContext = dbContext;
        _logger = logger;
        _teamsChallengerRepository = teamsChallengerRepository;
    }

    public async Task<CustomResponse<FinalChallengeSnapshot>> GetLiveSnapshotAsync(int eventId, int? currentUserId)
    {
        var context = await BuildChallengeContextAsync(eventId, currentUserId);
        if (context.Error is not null)
            return CustomResponse<FinalChallengeSnapshot>.Fail(context.Error);

        var result = new FinalChallengeSnapshot
        {
            Event = context.EventWindow!,
            Summary = BuildSummary(context),
            Clans = context.Clans,
            Tasks = BuildTasks(context, context.CurrentUserClanId ?? context.Clans.FirstOrDefault()?.ClanId),
            Feed = BuildFeed(context)
        };

        return CustomResponse<FinalChallengeSnapshot>.SuccessTrade(result);
    }

    public async Task<CustomResponse<IEnumerable<FinalChallengeClan>>> GetLeaderboardAsync(int eventId, int? currentUserId)
    {
        var context = await BuildChallengeContextAsync(eventId, currentUserId);
        if (context.Error is not null)
            return CustomResponse<IEnumerable<FinalChallengeClan>>.Fail(context.Error);

        return CustomResponse<IEnumerable<FinalChallengeClan>>.SuccessTrade(context.Clans);
    }

    public async Task<CustomResponse<IEnumerable<FinalChallengeTask>>> GetClanTasksAsync(int eventId, int clanId)
    {
        var context = await BuildChallengeContextAsync(eventId, null);
        if (context.Error is not null)
            return CustomResponse<IEnumerable<FinalChallengeTask>>.Fail(context.Error);

        var clanExists = context.TeamMetrics.ContainsKey(clanId);
        if (!clanExists)
            return CustomResponse<IEnumerable<FinalChallengeTask>>.Fail("Clã não encontrado para este evento.");

        return CustomResponse<IEnumerable<FinalChallengeTask>>.SuccessTrade(BuildTasks(context, clanId));
    }

    public async Task<CustomResponse<IEnumerable<FinalChallengeFeedItem>>> GetActivityFeedAsync(int eventId)
    {
        var context = await BuildChallengeContextAsync(eventId, null);
        if (context.Error is not null)
            return CustomResponse<IEnumerable<FinalChallengeFeedItem>>.Fail(context.Error);

        return CustomResponse<IEnumerable<FinalChallengeFeedItem>>.SuccessTrade(BuildFeed(context));
    }

    public async Task<CustomResponse<FinalChallengeSubmitResult>> SubmitAnswerAsync(FinalChallengeAnswerRequest answerRequest, int? currentUserId)
    {
        if (currentUserId is null)
            return CustomResponse<FinalChallengeSubmitResult>.Fail("Usuário não autenticado.");

        if (string.IsNullOrWhiteSpace(answerRequest.Answer))
            return CustomResponse<FinalChallengeSubmitResult>.Fail("A resposta deve ser preenchida.");

        var exercise = await _dbContext.Exercises
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == answerRequest.ChallengeId);

        if (exercise is null)
            return CustomResponse<FinalChallengeSubmitResult>.Fail("Desafio não encontrado.");

        var question = await _dbContext.Questions
            .AsNoTracking()
            .Where(x => x.ExerciseId == answerRequest.ChallengeId)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (question is null)
            return CustomResponse<FinalChallengeSubmitResult>.Fail("O desafio não possui pergunta cadastrada.");

        var answer = new Answer
        {
            UserId = currentUserId.Value,
            QuestionId = question.Id,
            ExerciseId = answerRequest.ChallengeId,
            AnswerText = answerRequest.Answer.Trim(),
            SelectedOption = 0,
            IsCorrect = false,
            AnsweredAt = DateTime.UtcNow,
            Feedback = answerRequest.Attachments.Count > 0
                ? $"Attachments: {string.Join(", ", answerRequest.Attachments)}"
                : null
        };

        _dbContext.Answers.Add(answer);
        await _dbContext.SaveChangesAsync();

        var result = new FinalChallengeSubmitResult
        {
            SubmissionId = answer.Id,
            Status = "queued",
            Message = "Resposta enviada com sucesso e aguardando validação."
        };

        return CustomResponse<FinalChallengeSubmitResult>.SuccessTrade(result);
    }

    private async Task<ChallengeContext> BuildChallengeContextAsync(int eventId, int? currentUserId)
    {
        var rankingEvent = await _dbContext.RankingEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == eventId);

        var teams = await _dbContext.TeamsChallengers
            .AsNoTracking()
            .Where(x => x.ModuleId == eventId)
            .Include(x => x.Module)
            .Include(x => x.Class)
            .OrderBy(x => x.Name)
            .ToListAsync();

        var moduleId = teams.FirstOrDefault()?.ModuleId ?? eventId;
        if (teams.Count == 0)
        {
            teams = await _dbContext.TeamsChallengers
                .AsNoTracking()
                .Where(x => x.ModuleId == moduleId)
                .Include(x => x.Module)
                .Include(x => x.Class)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        if (teams.Count == 0)
        {
            return new ChallengeContext
            {
                Error = "Nenhum clã foi encontrado para este evento."
            };
        }

        var teamIds = teams.Select(x => x.Id).ToList();

        var members = await _dbContext.MembersChallengers
            .AsNoTracking()
            .Where(x => teamIds.Contains(x.TeamId))
            .Include(x => x.User)
            .ToListAsync();

        var phaseIds = await _dbContext.Phases
            .AsNoTracking()
            .Where(x => x.ModuleId == moduleId)
            .Select(x => x.Id)
            .ToListAsync();

        var exercises = await _dbContext.Exercises
            .AsNoTracking()
            .Where(x => phaseIds.Contains(x.PhaseId) && x.IsFinalExercise)
            .Include(x => x.Category)
            .OrderBy(x => x.IndexOrder)
            .ThenBy(x => x.Id)
            .ToListAsync();

        var exerciseIds = exercises.Select(x => x.Id).ToList();
        var memberUserIds = members.Select(x => x.UserId).Distinct().ToList();

        var questions = exerciseIds.Count == 0
            ? new List<Question>()
            : await _dbContext.Questions
                .AsNoTracking()
                .Where(x => exerciseIds.Contains(x.ExerciseId))
                .ToListAsync();

        var answers = memberUserIds.Count == 0 || exerciseIds.Count == 0
            ? new List<Answer>()
            : await _dbContext.Answers
                .AsNoTracking()
                .Where(x => memberUserIds.Contains(x.UserId) && exerciseIds.Contains(x.ExerciseId))
                .ToListAsync();

        var progresses = memberUserIds.Count == 0 || exerciseIds.Count == 0
            ? new List<ProgressExerciseStudent>()
            : await _dbContext.ProgressExerciseStudents
                .AsNoTracking()
                .Where(x => memberUserIds.Contains(x.UserId) && exerciseIds.Contains(x.ExerciseId))
                .ToListAsync();

        var submissions = await _dbContext.FinalModuleSubmissions
            .AsNoTracking()
            .Where(x => x.ModuleId == moduleId && teamIds.Contains(x.TeamId))
            .OrderByDescending(x => x.SubmissionDate)
            .ToListAsync();

        var currentUserClanId = currentUserId is null
            ? null
            : members.FirstOrDefault(x => x.UserId == currentUserId.Value)?.TeamId;

        var eventWindow = BuildEventWindow(eventId, rankingEvent, teams.First(), moduleId);
        var teamMetrics = BuildTeamMetrics(teams, members, exercises, answers, progresses, submissions, eventWindow.StartsAt, currentUserClanId);
        var clans = BuildClans(teamMetrics, exercises.Count);

        return new ChallengeContext
        {
            EventId = eventId,
            ModuleId = moduleId,
            EventWindow = eventWindow,
            Teams = teams,
            Members = members,
            Exercises = exercises,
            Questions = questions,
            Answers = answers,
            Progresses = progresses,
            Submissions = submissions,
            CurrentUserClanId = currentUserClanId,
            TeamMetrics = teamMetrics,
            Clans = clans
        };
    }

    private static FinalChallengeEventWindow BuildEventWindow(int eventId, RankingEvent? rankingEvent, TeamsChallenger fallbackTeam, int moduleId)
    {
        var now = DateTime.UtcNow;
        var startsAt = rankingEvent?.StartTime.ToUniversalTime() ?? now;
        var endsAt = rankingEvent is null
            ? now.AddHours(2)
            : startsAt.AddMinutes(rankingEvent.DurationMinutes);

        var status = now < startsAt
            ? "scheduled"
            : now <= endsAt
                ? "active"
                : "closed";

        return new FinalChallengeEventWindow
        {
            EventId = eventId,
            Title = rankingEvent?.EventName ?? $"Desafio final do módulo {moduleId}",
            ModuleName = fallbackTeam.Module?.Name ?? $"Módulo {moduleId}",
            ClassName = fallbackTeam.Class?.Name ?? "Turma não informada",
            Status = status,
            StartsAt = startsAt,
            EndsAt = endsAt,
            ServerTime = now,
            RefreshIntervalSeconds = 10,
            UpdateMode = "polling"
        };
    }

    private static Dictionary<int, TeamMetric> BuildTeamMetrics(
        List<TeamsChallenger> teams,
        List<MembersChallenger> members,
        List<Exercise> exercises,
        List<Answer> answers,
        List<ProgressExerciseStudent> progresses,
        List<FinalModuleSubmission> submissions,
        DateTime startsAt,
        int? currentUserClanId)
    {
        var metrics = new Dictionary<int, TeamMetric>();
        var totalChallenges = exercises.Count;

        foreach (var team in teams)
        {
            var teamMembers = members.Where(x => x.TeamId == team.Id).ToList();
            var userIds = teamMembers.Select(x => x.UserId).Distinct().ToHashSet();
            var teamAnswers = answers.Where(x => userIds.Contains(x.UserId)).ToList();
            var teamProgresses = progresses.Where(x => userIds.Contains(x.UserId)).ToList();
            var teamSubmissions = submissions.Where(x => x.TeamId == team.Id).ToList();

            var solvedExerciseIds = teamProgresses
                .Where(x => x.IsCompleted)
                .Select(x => x.ExerciseId)
                .Distinct()
                .ToList();

            var openExercise = exercises
                .FirstOrDefault(exercise => !solvedExerciseIds.Contains(exercise.Id) && !teamAnswers.Any(answer => answer.ExerciseId == exercise.Id));

            var avgResolutionSeconds = teamAnswers.Count == 0
                ? 0
                : teamAnswers
                    .Select(x => Math.Max(0, (x.AnsweredAt.ToUniversalTime() - startsAt).TotalSeconds))
                    .Average();

            var averageAiScore = teamProgresses.Count == 0
                ? (teamSubmissions.Count == 0 ? 0 : teamSubmissions.Average(x => x.Score))
                : teamProgresses.Average(x => x.Score);

            var totalScore = teamSubmissions.Sum(x => x.Score) + teamProgresses.Sum(x => x.Score);
            var progressPercent = totalChallenges == 0 ? 0 : solvedExerciseIds.Count * 100d / totalChallenges;
            var recentStreak = teamAnswers
                .Where(x => x.AnsweredAt.ToUniversalTime() >= DateTime.UtcNow.AddHours(-1))
                .Select(x => x.ExerciseId)
                .Distinct()
                .Count();

            metrics[team.Id] = new TeamMetric
            {
                Team = team,
                Members = teamMembers,
                Answers = teamAnswers,
                Progresses = teamProgresses,
                Submissions = teamSubmissions,
                SolvedExerciseIds = solvedExerciseIds,
                TotalScore = totalScore,
                AverageAiScore = averageAiScore,
                AverageResolutionSeconds = avgResolutionSeconds,
                ProgressPercent = progressPercent,
                CurrentCheckpoint = openExercise?.Title ?? "Concluído",
                IsCurrentUserClan = currentUserClanId == team.Id,
                Streak = recentStreak
            };
        }

        return metrics;
    }

    private static List<FinalChallengeClan> BuildClans(Dictionary<int, TeamMetric> teamMetrics, int totalChallenges)
    {
        return teamMetrics.Values
            .OrderByDescending(x => x.TotalScore)
            .ThenByDescending(x => x.SolvedExerciseIds.Count)
            .ThenBy(x => x.Team.Name)
            .Select((metric, index) => new FinalChallengeClan
            {
                ClanId = metric.Team.Id,
                Name = metric.Team.Name,
                Motto = metric.Team.Description,
                Color = metric.Team.ClanColor,
                BoatName = metric.Team.BoatName,
                MembersCount = metric.Members.Count,
                SolvedCount = metric.SolvedExerciseIds.Count,
                TotalScore = Math.Round(metric.TotalScore, 2),
                AverageAiScore = Math.Round(metric.AverageAiScore, 2),
                AverageResolutionSeconds = Math.Round(metric.AverageResolutionSeconds, 2),
                ProgressPercent = Math.Round(metric.ProgressPercent, 2),
                DistanceToFinishPercent = Math.Round(Math.Max(0, 100 - metric.ProgressPercent), 2),
                CurrentCheckpoint = metric.CurrentCheckpoint,
                X = 10 + (index * 16),
                Y = Math.Clamp(100 - metric.ProgressPercent, 8, 92),
                IsCurrentUserClan = metric.IsCurrentUserClan,
                Streak = metric.Streak
            })
            .ToList();
    }

    private static FinalChallengeSummary BuildSummary(ChallengeContext context)
    {
        var currentUserClan = context.CurrentUserClanId is null
            ? null
            : context.Clans.FirstOrDefault(x => x.ClanId == context.CurrentUserClanId.Value);

        var currentUserClanRank = currentUserClan is null
            ? 0
            : context.Clans.FindIndex(x => x.ClanId == currentUserClan.ClanId) + 1;

        return new FinalChallengeSummary
        {
            TotalClans = context.Clans.Count,
            TotalParticipants = context.Members.Select(x => x.UserId).Distinct().Count(),
            TotalChallenges = context.Exercises.Count,
            ValidatedSubmissions = context.Submissions.Count + context.Progresses.Count(x => x.IsCompleted),
            CurrentUserClanRank = currentUserClanRank,
            CurrentUserClanScore = currentUserClan?.TotalScore ?? 0
        };
    }

    private static List<FinalChallengeTask> BuildTasks(ChallengeContext context, int? clanId)
    {
        TeamMetric? metric = null;
        if (clanId is not null)
            context.TeamMetrics.TryGetValue(clanId.Value, out metric);

        return context.Exercises.Select(exercise =>
        {
            var progressForTask = metric?.Progresses
                .Where(x => x.ExerciseId == exercise.Id)
                .OrderByDescending(x => x.Score)
                .FirstOrDefault();

            var answersForTask = metric?.Answers
                .Where(x => x.ExerciseId == exercise.Id)
                .OrderByDescending(x => x.AnsweredAt)
                .ToList() ?? new List<Answer>();

            var status = progressForTask?.IsCompleted == true
                ? "validated"
                : answersForTask.Count > 0
                    ? "submitted"
                    : context.EventWindow!.Status == "closed"
                        ? "locked"
                        : "open";

            return new FinalChallengeTask
            {
                ChallengeId = exercise.Id,
                Title = exercise.Title ?? $"Desafio {exercise.Id}",
                Category = exercise.Category?.Name ?? "Sem categoria",
                Briefing = context.Questions.FirstOrDefault(x => x.ExerciseId == exercise.Id)?.Statement ?? exercise.Description,
                Difficulty = MapDifficulty(exercise.Difficulty),
                Status = status,
                ScoreWeight = exercise.PointsRedeem,
                DeadlineAt = exercise.TermAt,
                BestSubmissionAt = answersForTask.FirstOrDefault()?.AnsweredAt,
                AiScore = progressForTask?.Score
            };
        }).ToList();
    }

    private static List<FinalChallengeFeedItem> BuildFeed(ChallengeContext context)
    {
        var submissionFeed = context.Submissions.Select(submission => new FinalChallengeFeedItem
        {
            Id = $"submission-{submission.Id}",
            ClanId = submission.TeamId,
            ClanName = context.TeamMetrics[submission.TeamId].Team.Name,
            Type = "validated",
            Title = "Entrega validada",
            Description = $"Pontuação {submission.Score} registrada para o módulo.",
            HappenedAt = submission.SubmissionDate
        });

        var answersFeed = context.Answers
            .OrderByDescending(x => x.AnsweredAt)
            .Take(20)
            .Select(answer =>
            {
                var clan = context.TeamMetrics.Values.FirstOrDefault(metric => metric.Members.Any(member => member.UserId == answer.UserId))?.Team;
                if (clan is null)
                    return null;

                var exerciseTitle = context.Exercises.FirstOrDefault(x => x.Id == answer.ExerciseId)?.Title ?? $"Desafio {answer.ExerciseId}";

                return new FinalChallengeFeedItem
                {
                    Id = $"answer-{answer.Id}",
                    ClanId = clan.Id,
                    ClanName = clan.Name,
                    Type = answer.IsCorrect ? "validated" : "submission",
                    Title = answer.IsCorrect ? "Resposta validada" : "Nova submissão",
                    Description = $"{clan.Name} enviou resposta para {exerciseTitle}.",
                    HappenedAt = answer.AnsweredAt
                };
            })
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

        return submissionFeed
            .Concat(answersFeed)
            .OrderByDescending(x => x.HappenedAt)
            .Take(20)
            .ToList();
    }

    private static string MapDifficulty(DifficultyLevel difficulty)
    {
        return difficulty switch
        {
            DifficultyLevel.Lower => "easy",
            DifficultyLevel.Normal => "medium",
            DifficultyLevel.ProofTest => "hard",
            DifficultyLevel.RepeatGenerate => "boss",
            _ => "medium"
        };
    }

    public async Task<CustomResponse<TeamsUserChallengerDTO>> GetTeamForPlayerRelationship(int classId, int moduleId, int? currentUserId)
    {
        try
        {
            if (currentUserId is null)
                return CustomResponse<TeamsUserChallengerDTO>.Fail("Usuário não autenticado.");

            var team = await _teamsChallengerRepository.GetTeamForPlayerRelationshipAsync(classId, moduleId, currentUserId.Value);
            if (team is null)
                return CustomResponse<TeamsUserChallengerDTO>.Fail("Nenhum relacionamento encontrado para o usuário neste módulo e turma.");

            var playerTeamsMounted = await _teamsChallengerRepository.GetUsersOfTeamAsync(team.Id);

            var result = new TeamsUserChallengerDTO
            {
                ClanName = team.Name,
                ClanDescription = team.Description,
                ClanColor = team.ClanColor,
                BoatName = team.BoatName,
                Members = playerTeamsMounted.Select(member => new TeamsClanMemberDTO
                {
                    UserId = member.UserId,
                    UserName = member.User.Name,
                    UserEmail = member.User.Email,
                    UserAvatarUrl = member.User.ProfilePictureUrl,
                    IsCurrentUser = member.UserId == currentUserId.Value
                }).ToList()
            };

            return CustomResponse<TeamsUserChallengerDTO>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter relacionamento do usuário com o time no desafio final.");
            return CustomResponse<TeamsUserChallengerDTO>.Fail("Ocorreu um erro ao processar a solicitação.");
        }
    }

    public async Task<CustomResponse<CreateTeamRequest>> CreateTeamAsync(CreateTeamRequest createTeamRequest, int? currentUserId)
    {
        try
        {
            if (currentUserId is null)
                return CustomResponse<CreateTeamRequest>.Fail("Usuário não autenticado.");

            var result = await _teamsChallengerRepository.CreateTeamAsync(createTeamRequest, currentUserId.Value);
            if (result is null)
                return CustomResponse<CreateTeamRequest>.Fail("Falha ao criar time.");

            return CustomResponse<CreateTeamRequest>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar time para o desafio final.");
            return CustomResponse<CreateTeamRequest>.Fail("Ocorreu um erro ao processar a solicitação.");
        }
    }
}
