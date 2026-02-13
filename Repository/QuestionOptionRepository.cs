using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class QuestionOptionRepository : IQuestionOptionRepository
{
    private readonly AppDbContext _efDbContext;

    public QuestionOptionRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<QuestionOption>> GetAllAsync()
    {
        return await _efDbContext.QuestionOptions.AsNoTracking().ToListAsync();
    }

    public async Task<QuestionOption?> GetByIdAsync(int id)
    {
        return await _efDbContext.QuestionOptions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }
}
