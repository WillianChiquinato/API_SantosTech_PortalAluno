using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class ExerciseRepository : IExerciseRepository
{
    private readonly AppDbContext _efDbContext;

    public ExerciseRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<Exercise>> GetAllAsync()
    {
        return await _efDbContext.Exercises.AsNoTracking().ToListAsync();
    }

    public async Task<Exercise?> GetByIdAsync(int id)
    {
        return await _efDbContext.Exercises.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }
}
