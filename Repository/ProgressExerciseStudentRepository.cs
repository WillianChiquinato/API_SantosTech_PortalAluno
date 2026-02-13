using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class ProgressExerciseStudentRepository : IProgressExerciseStudentRepository
{
    private readonly AppDbContext _efDbContext;

    public ProgressExerciseStudentRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<ProgressExerciseStudent>> GetAllAsync()
    {
        return await _efDbContext.ProgressExerciseStudents.AsNoTracking().ToListAsync();
    }

    public async Task<ProgressExerciseStudent?> GetByIdAsync(int id)
    {
        return await _efDbContext.ProgressExerciseStudents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }
}
