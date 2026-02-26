using SendGrid;
using SendGrid.Helpers.Mail;
using API_PortalSantosTech.Interfaces;

namespace API_PortalSantosTech.Services;

public class SendGridEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public SendGridEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        var apiKey = _configuration["SendGrid:ApiKey"];
        var client = new SendGridClient(apiKey);

        var from = new EmailAddress(
            _configuration["SendGrid:FromEmail"],
            _configuration["SendGrid:FromName"]
        );

        var to = new EmailAddress(toEmail);

        var msg = MailHelper.CreateSingleEmail(
            from,
            to,
            subject,
            "",
            htmlContent
        );

        var response = await client.SendEmailAsync(msg);

        return response.IsSuccessStatusCode;
    }
}