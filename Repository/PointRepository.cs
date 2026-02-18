using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class PointRepository : IPointRepository
{
    private readonly AppDbContext _efDbContext;

    public PointRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<Point>> GetAllAsync()
    {
        return await _efDbContext.Points.AsNoTracking().ToListAsync();
    }

    public async Task<Point?> GetByIdAsync(int id)
    {
        return await _efDbContext.Points.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<List<Point>> GetRankingAsync()
    {
        //Agrupar por usuario.
        return _efDbContext.Points
            .AsNoTracking()
            .GroupBy(p => p.UserId)
            .Select(g => new Point
            {
                UserId = g.Key,
                Points = g.Sum(p => p.Points)
            })
            .OrderByDescending(p => p.Points)
            .ToListAsync();
    }
}
