using API_PortalSantosTech.Data;
using API_PortalSantosTech.DependencyInjection;
using Amazon;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using API_PortalSantosTech.Services;
using System.Threading.RateLimiting;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsProduction())
{
    builder.WebHost.UseUrls("http://0.0.0.0:8080");
}

var connectionString =
    $"Host={Environment.GetEnvironmentVariable("DB_SERVER")};" +
    $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
    $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
    $"Username={Environment.GetEnvironmentVariable("DB_USER")};" +
    $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};" +
    $"Ssl Mode={Environment.GetEnvironmentVariable("DB_SSL")};";


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddScoped<TokenService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.TagActionsBy(api =>
    {
        if (!string.IsNullOrWhiteSpace(api.GroupName))
            return new[] { api.GroupName };

        return new[] { api.ActionDescriptor.RouteValues["controller"] };
    });

    c.OrderActionsBy(apiDesc => apiDesc.GroupName);
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "rate_limit_exceeded",
            message = "Muitas tentativas de login. Aguarde antes de tentar novamente."
        }, cancellationToken);
    };

    options.AddPolicy("loginPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddProjectDependencies();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IAmazonS3>(_ =>
{
    var accountId = Environment.GetEnvironmentVariable("CLOUDFLARE_ACCOUNT_ID")
                    ?? throw new InvalidOperationException("Variável CLOUDFLARE_ACCOUNT_ID não configurada.");
    var accessKeyId = Environment.GetEnvironmentVariable("CLOUDFLARE_ACCESS_KEY_ID")
                      ?? throw new InvalidOperationException("Variável CLOUDFLARE_ACCESS_KEY_ID não configurada.");
    var secretAccessKey = Environment.GetEnvironmentVariable("CLOUDFLARE_SECRET_ACCESS_KEY")
                          ?? throw new InvalidOperationException("Variável CLOUDFLARE_SECRET_ACCESS_KEY não configurada.");

    var s3Config = new AmazonS3Config
    {
        ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
        ForcePathStyle = true,
        AuthenticationRegion = "auto"
    };

    return new AmazonS3Client(accessKeyId, secretAccessKey, s3Config);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            )
        };
    });

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok("API Running"));
app.Run();