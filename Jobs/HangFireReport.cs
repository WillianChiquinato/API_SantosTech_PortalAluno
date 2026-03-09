using Hangfire;

namespace API_PortalSantosTech.Services;

public static class HangfireJobs
{
    public static void Register()
    {
        RecurringJob.AddOrUpdate<ReportService>(
            "weekly-performance-report",
            x => x.SendWeeklyReport(),
            Cron.Weekly(DayOfWeek.Monday, 8)
        );
    }
}