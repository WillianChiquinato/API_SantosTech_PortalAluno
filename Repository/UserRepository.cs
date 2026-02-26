using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Services;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _efDbContext;
    private readonly IEmailService _emailService;

    public UserRepository(AppDbContext efDbContext, IEmailService emailService)
    {
        _efDbContext = efDbContext;
        _emailService = emailService;
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _efDbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email);
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

    public async Task<bool> SendEmailVerifyAsync(string email)
    {
        if (email == null)
        {
            return false;
        }

        var responseGrid = await _emailService.SendEmailAsync(email, "Verificação de Email", "<p>Por favor, verifique seu email.</p>");

        if (responseGrid)
        {
            //Update database config receive email notifications to true.
            var user = await _efDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)            {
                var config = await _efDbContext.Configs.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (config != null)
                {
                    config.ReceiveEmailNotifications = true;
                    _efDbContext.Configs.Update(config);
                    await _efDbContext.SaveChangesAsync();
                }
            }
        }

        return responseGrid;
    }
}
