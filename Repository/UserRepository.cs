using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;

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
        var user = await _efDbContext.Configs.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
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
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var verificationCode = GenerateVerificationCode();

        var codeEmail = new CodeEmail
        {
            Email = email,
            Code = verificationCode,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _efDbContext.CodeEmails.Add(codeEmail);
        await _efDbContext.SaveChangesAsync();

        var htmlBody = BuildEmailVerificationHtml(verificationCode);
        var responseGrid = await _emailService.SendEmailAsync(
            email,
            "Verificação de Email",
            htmlBody
        );

        return responseGrid;
    }

    private static string GenerateVerificationCode()
    {
        return Random.Shared.Next(0, 1_000_000).ToString("D6");
    }

    private static string BuildEmailVerificationHtml(string verificationCode)
    {
        var html = new StringBuilder();

        html.Append("<!DOCTYPE html>");
        html.Append("<html lang='pt-BR'>");
        html.Append("<head>");
        html.Append("<meta charset='UTF-8' />");
        html.Append("<meta name='viewport' content='width=device-width, initial-scale=1.0' />");
        html.Append("<title>Verificacao de Email</title>");
        html.Append("</head>");
        html.Append("<body style='margin:0;padding:0;background:#f3f6fb;font-family:Arial,sans-serif;'>");
        html.Append("<table role='presentation' cellpadding='0' cellspacing='0' width='100%' style='padding:24px 12px;'>");
        html.Append("<tr><td align='center'>");
        html.Append("<table role='presentation' cellpadding='0' cellspacing='0' width='100%' style='max-width:560px;background:#ffffff;border-radius:16px;overflow:hidden;border:1px solid #e5e7eb;'>");
        html.Append("<tr><td style='background:linear-gradient(135deg,#0f766e,#0891b2);padding:28px 24px;text-align:center;'>");
        html.Append("<h1 style='margin:0;color:#ffffff;font-size:24px;line-height:1.2;'>Confirme seu email</h1>");
        html.Append("<p style='margin:10px 0 0;color:#8c0808;font-size:25px;font-weight:700;'>Portal Santos Tech</p>");
        html.Append("</td></tr>");
        html.Append("<tr><td style='padding:28px 24px 12px;'>");
        html.Append("<p style='margin:0 0 16px;color:#111827;font-size:16px;line-height:1.6;'>Use o codigo abaixo para concluir a verificacao da sua conta:</p>");
        html.Append("<div style='margin:18px 0 22px;padding:18px 12px;border:1px dashed #14b8a6;border-radius:12px;background:#f0fdfa;text-align:center;'>");
        html.Append("<span style='display:inline-block;letter-spacing:8px;font-size:34px;font-weight:700;color:#0f172a;'>");
        html.Append(verificationCode);
        html.Append("</span>");
        html.Append("</div>");
        html.Append("<p style='margin:0;color:#374151;font-size:14px;line-height:1.6;'>Este codigo expira em 10 minutos e pode ser usado apenas uma vez.</p>");
        html.Append("</td></tr>");
        html.Append("<tr><td style='padding:10px 24px 28px;'>");
        html.Append("<p style='margin:0;color:#6b7280;font-size:12px;line-height:1.6;'>Se voce nao solicitou esta verificacao, ignore este email.</p>");
        html.Append("</td></tr>");
        html.Append("</table>");
        html.Append("</td></tr>");
        html.Append("</table>");
        html.Append("</body>");
        html.Append("</html>");

        return html.ToString();
    }

    public async Task<bool> ConfirmEmailVerifyAsync(string email, string code)
    {
        var codeEntry = await _efDbContext.CodeEmails.FirstOrDefaultAsync(c => c.Email == email && c.Code == code);
        if (codeEntry == null || codeEntry.CreatedAt.AddMinutes(10) < DateTime.UtcNow)
        {
            return false;
        }

        _efDbContext.CodeEmails.Remove(codeEntry);
        await _efDbContext.SaveChangesAsync();

        var user = await _efDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null)
        {
            var config = await _efDbContext.Configs.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (config != null)
            {
                config.ReceiveEmailNotifications = false;
                _efDbContext.Configs.Update(config);
                await _efDbContext.SaveChangesAsync();
            }
        }

        return true;
    }

    public async Task<PasswordRecoveryResult> SendPasswordRecoveryEmailAsync(string email)
    {
        var user = _efDbContext.Users.AsNoTracking().FirstOrDefault(u => u.Email == email);
        if (user == null)
        {
            return new PasswordRecoveryResult { Success = false };
        }

        var recoveryCode = GeneratePasswordRecoveryCode();
        var htmlBody = BuildPasswordRecoveryEmailHtml(recoveryCode, user.Name!);

        var responseGrid = await _emailService.SendEmailAsync(
            email,
            "Recuperação de Senha",
            htmlBody
        );

        return responseGrid ? new PasswordRecoveryResult { Success = true, PasswordRecoveryCode = recoveryCode } : new PasswordRecoveryResult { Success = false };
    }

    private static string GeneratePasswordRecoveryCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static string BuildPasswordRecoveryEmailHtml(string recoveryCode, string userName)
    {
        var safeUserName = WebUtility.HtmlEncode(userName);
        var html = new StringBuilder();

        html.Append("<!DOCTYPE html>");
        html.Append("<html lang='pt-BR'>");
        html.Append("<head>");
        html.Append("<meta charset='UTF-8' />");
        html.Append("<meta name='viewport' content='width=device-width, initial-scale=1.0' />");
        html.Append("<title>Recuperação de Senha</title>");
        html.Append("</head>");
        html.Append("<body style='margin:0;padding:0;background:#eef3fb;font-family:Segoe UI,Arial,sans-serif;color:#0f172a;'>");
        html.Append("<table role='presentation' cellpadding='0' cellspacing='0' width='100%' style='background:#eef3fb;padding:32px 14px;'>");
        html.Append("<tr><td align='center'>");
        html.Append("<table role='presentation' cellpadding='0' cellspacing='0' width='100%' style='max-width:600px;background:#ffffff;border-radius:18px;overflow:hidden;border:1px solid #dbe6f2;box-shadow:0 12px 30px rgba(15,23,42,0.10);'>");

        html.Append("<tr><td style='background:#052e4f;background-image:linear-gradient(135deg,#0b3a63,#0b6aa2);padding:34px 28px;text-align:left;'>");
        html.Append("<p style='margin:0 0 8px;color:#cde9ff;font-size:12px;letter-spacing:1.2px;text-transform:uppercase;font-weight:700;'>Portal Santos Tech</p>");
        html.Append("<h1 style='margin:0;color:#ffffff;font-size:26px;line-height:1.3;font-weight:700;'>Recuperação de senha</h1>");
        html.Append($"<p style='margin:12px 0 0;color:#e2f1ff;font-size:16px;line-height:1.5;'>Olá, <strong>{safeUserName}</strong>. Recebemos uma solicitação para redefinir sua senha.</p>");
        html.Append("</td></tr>");

        html.Append("<tr><td style='padding:30px 28px 8px;'>");
        html.Append("<p style='margin:0 0 14px;color:#1e293b;font-size:16px;line-height:1.7;'>Use o código de verificação abaixo para concluir a recuperação da sua senha:</p>");
        html.Append("<table role='presentation' cellpadding='0' cellspacing='0' width='100%' style='margin:0 0 18px;border:1px solid #c7dcf3;border-radius:14px;background:#f6fbff;'>");
        html.Append("<tr><td style='padding:20px 16px;text-align:center;'>");
        html.Append("<p style='margin:0 0 8px;color:#0b6aa2;font-size:12px;letter-spacing:1px;text-transform:uppercase;font-weight:700;'>Código de recuperação</p>");
        html.Append("<span style='display:inline-block;letter-spacing:9px;font-size:36px;font-weight:800;color:#0f172a;'>");
        html.Append(recoveryCode);
        html.Append("</span>");
        html.Append("</td></tr>");
        html.Append("</table>");
        html.Append("<p style='margin:0 0 8px;color:#334155;font-size:14px;line-height:1.6;'><strong>Validade:</strong> este código expira em 15 minutos.</p>");
        html.Append("<p style='margin:0;color:#334155;font-size:14px;line-height:1.6;'>Se você não solicitou esta ação, ignore este e-mail com segurança.</p>");
        html.Append("</td></tr>");

        html.Append("<tr><td style='padding:20px 28px 28px;'>");
        html.Append("<table role='presentation' cellpadding='0' cellspacing='0' width='100%' style='border-top:1px solid #e2e8f0;padding-top:18px;'>");
        html.Append("<tr><td style='color:#64748b;font-size:12px;line-height:1.6;'>");
        html.Append("Este é um e-mail automático. Não responda esta mensagem.<br />");
        html.Append("Suporte Portal Santos Tech");
        html.Append("</td></tr>");
        html.Append("</table>");
        html.Append("</td></tr>");
        html.Append("</table>");
        html.Append("</td></tr>");
        html.Append("</table>");
        html.Append("</body>");
        html.Append("</html>");

        return html.ToString();
    }

    public async Task UpdatePasswordHashAsync(string email, string? passwordHash)
    {
        var user = await _efDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null)
        {
            user.PasswordHash = passwordHash;
            _efDbContext.Users.Update(user);
            await _efDbContext.SaveChangesAsync();
        }
    }
}
