using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class LogsRepository : ILogsRepository
{
    private readonly AppDbContext _efDbContext;

    public LogsRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<Logs>> GetLogsAsync()
    {
        return await _efDbContext.Logs.AsNoTracking().ToListAsync();
    }

    public async Task<List<Logs?>> GetLogsByUserIdAsync(int userId)
    {
        return await _efDbContext.Logs.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();
    }

    public async Task AddLogsAsync(Logs log)
    {
        await _efDbContext.Logs.AddAsync(log);
        
        await _efDbContext.SaveChangesAsync();
    }
}