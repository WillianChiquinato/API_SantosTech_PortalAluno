using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class MembersChallengerRepository : IMembersChallengerRepository
{
    private readonly AppDbContext _efDbContext;

    public MembersChallengerRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<MembersChallenger>> GetAllAsync()
    {
        return await _efDbContext.MembersChallengers.AsNoTracking().ToListAsync();
    }

    public async Task<MembersChallenger?> GetByIdAsync(int id)
    {
        return await _efDbContext.MembersChallengers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }
}
