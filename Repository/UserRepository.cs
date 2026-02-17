using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _efDbContext;

    public UserRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<User?> GetUserByEmailAndPassword(string email, string password)
    {
        return await _efDbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email && x.PasswordHash == password);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _efDbContext.Users.AsNoTracking().ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _efDbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }
}
