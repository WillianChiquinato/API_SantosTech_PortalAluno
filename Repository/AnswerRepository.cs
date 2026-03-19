using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class AnswerRepository : IAnswerRepository
{
    private readonly AppDbContext _efDbContext;

    public AnswerRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<Answer>> GetAllAsync()
    {
        return await _efDbContext.Answers.AsNoTracking().ToListAsync();
    }

    public async Task<Answer?> GetByIdAsync(int id)
    {
        return await _efDbContext.Answers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Answer>> GetByUserIdAsync(int userId)
    {
        return await _efDbContext.Answers.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();
    }

    public Task<Dictionary<int, Exercise>> GetExercisesByAnswerIdsAsync(List<int> answerIds)
    {
        return _efDbContext.Answers
            .Where(a => answerIds.Contains(a.Id))
            .Select(a => new { a.Id, a.ExerciseId })
            .Join(_efDbContext.Exercises, a => a.ExerciseId, e => e.Id, (a, e) => new { a.Id, Exercise = e })
            .ToDictionaryAsync(x => x.Id, x => x.Exercise);
    }

    public async Task<int> GetNewAnswersByUserIdCount(int userId)
    {
        var userLasSeen = await _efDbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => u.LastSeenAt)
            .FirstOrDefaultAsync();

        return await _efDbContext.Answers
            .Where(a => a.UserId == userId && a.AnsweredAt > userLasSeen)
            .CountAsync();
    }

    public async Task<Dictionary<int, string>> GetSelectedOptionTextsByAnswerIdsAsync(List<int> answerIds)
    {
        return await _efDbContext.Answers
            .Where(a => answerIds.Contains(a.Id))
            .Select(a => new { a.Id, a.SelectedOption })
            .Join(_efDbContext.QuestionOptions, a => a.SelectedOption, qo => qo.Id, (a, qo) => new { a.Id, SelectedOptionText = qo.OptionText })
            .ToDictionaryAsync(x => x.Id, x => x.SelectedOptionText);
    }
}