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
}