using API_PortalSantosTech.Models;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {}

    public DbSet<Answer> Answers { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<BadgeStudent> BadgeStudents { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<DailyTask> DailyTasks { get; set; }
    public DbSet<FinalModuleSubmission> FinalModuleSubmissions { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<MembersChallenger> MembersChallengers { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<Phase> Phases { get; set; }
    public DbSet<Point> Points { get; set; }
    public DbSet<ProgressExerciseStudent> ProgressExerciseStudents { get; set; }
    public DbSet<ProgressStudentPhase> ProgressStudentPhases { get; set; }
    public DbSet<ProgressVideoStudent> ProgressVideoStudents { get; set; }
    public DbSet<ProgressPaidCourses> ProgressPaidCourses { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<TeamsChallenger> TeamsChallengers { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Video> Videos { get; set; }

    public DbSet<Logs> Logs { get; set; }
    public DbSet<LevelUser> LevelUsers { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Configs> Configs { get; set; }
}