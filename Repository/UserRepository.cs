using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
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

    public async Task<float> GetUserPointsAsync(int userId)
    {
        return await _efDbContext.Points.AsNoTracking()
            .Where(up => up.UserId == userId)
            .SumAsync(up => up.Points);
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        _efDbContext.Users.Update(user);
        await _efDbContext.SaveChangesAsync();
        
        return user;
    }

    public async Task<ConfigsDTO> GetConfigsAsync(int userId)
    {
        var user = await _efDbContext.Configs.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return null;
        }

        return new ConfigsDTO
        {
            ReceiveEmailNotifications = user.ReceiveEmailNotifications,
            DarkModeEnabled = user.DarkModeEnabled,
            ReportFrequency = user.ReportFrequency,
            AcessibilityMode = user.AcessibilityMode,
            PreferredLanguage = user.PreferredLanguage
        };
    }

    public async Task<ConfigsDTO> CreateNewConfigAsync(int userId)
    {
        var newConfig = new Configs
        {
            UserId = userId,
            ReceiveEmailNotifications = true,
            DarkModeEnabled = false,
            ReportFrequency = false,
            AcessibilityMode = false,
            PreferredLanguage = "PT"
        };

        _efDbContext.Configs.Add(newConfig);
        await _efDbContext.SaveChangesAsync();

        return new ConfigsDTO
        {
            ReceiveEmailNotifications = newConfig.ReceiveEmailNotifications,
            DarkModeEnabled = newConfig.DarkModeEnabled,
            ReportFrequency = newConfig.ReportFrequency,
            AcessibilityMode = newConfig.AcessibilityMode,
            PreferredLanguage = newConfig.PreferredLanguage
        };
    }

    public async Task<ConfigsDTO> UpdateConfigsAsync(UpdateConfigRequest request)
    {
        var config = await _efDbContext.Configs.FirstOrDefaultAsync(c => c.UserId == request.UserId);
        if (config == null)
        {
            return null;
        }

        config.ReceiveEmailNotifications = request.ReceiveEmailNotifications;
        config.DarkModeEnabled = request.DarkModeEnabled;
        config.ReportFrequency = request.ReportFrequency;
        config.AcessibilityMode = request.AcessibilityMode;
        config.PreferredLanguage = request.PreferredLanguage;

        _efDbContext.Configs.Update(config);
        await _efDbContext.SaveChangesAsync();
        return new ConfigsDTO
        {
            ReceiveEmailNotifications = config.ReceiveEmailNotifications,
            DarkModeEnabled = config.DarkModeEnabled,
            ReportFrequency = config.ReportFrequency,
            AcessibilityMode = config.AcessibilityMode,
            PreferredLanguage = config.PreferredLanguage
        };
    }
}
