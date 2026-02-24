namespace API_PortalSantosTech.Models.DTO;

public class ConfigsDTO
{
    public bool ReceiveEmailNotifications { get; set; }
    public bool DarkModeEnabled { get; set; }
    public bool ReportFrequency { get; set; } = false;
    public bool AcessibilityMode { get; set; } = false;
    public string PreferredLanguage { get; set; } = string.Empty;
}