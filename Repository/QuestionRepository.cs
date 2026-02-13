using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class QuestionRepository : IQuestionRepository
{
    private readonly AppDbContext _efDbContext;

    public QuestionRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<Question>> GetAllAsync()
    {
        return await _efDbContext.Questions.AsNoTracking().ToListAsync();
    }

    public async Task<Question?> GetByIdAsync(int id)
    {
        return await _efDbContext.Questions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }
}
