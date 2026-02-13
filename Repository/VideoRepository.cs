using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class VideoRepository : IVideoRepository
{
    private readonly AppDbContext _efDbContext;

    public VideoRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<Video>> GetAllAsync()
    {
        return await _efDbContext.Videos.AsNoTracking().ToListAsync();
    }

    public async Task<Video?> GetByIdAsync(int id)
    {
        return await _efDbContext.Videos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }
}
