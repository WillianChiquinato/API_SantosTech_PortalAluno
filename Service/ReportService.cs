using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Services;

public class ReportService
{
    private readonly IUserService _userService;
    private readonly AppDbContext _context;
    private readonly IEmailService _email;

    public ReportService(IUserService userService, AppDbContext context, IEmailService email)
    {
        _userService = userService;
        _context = context;
        _email = email;
    }

    public async Task SendWeeklyReport()
    {
        //Testes.
        var start = DateTime.UtcNow.AddDays(-7);

        var data = await _context.Users
            .Where(x => x.CreatedAt >= start)
            .ToListAsync();

        var report = $"Teste Relatório Semanal - {DateTime.UtcNow:yyyy-MM-dd}\n" +
                     $"Novos usuários na última semana: {data.Count}";

        var fullUsersResponse = await _userService.GetAllAsync();
        var configsAbility = await _userService.GetUsersWithAbilityAsync(fullUsersResponse.Result);

        foreach (var user in configsAbility.Result)
        {
            var configsVerify = user.Abilities!.FirstOrDefault(x => x.Contains("Relatório Semanal"));
            if (configsVerify != null)
            {
                await _email.SendEmailAsync(user.Email!, "Relatório Semanal", report);
            }
        }
    }
}