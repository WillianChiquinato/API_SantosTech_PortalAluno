using API_PortalSantosTech.Data;
using API_PortalSantosTech.DependencyInjection;
using API_PortalSantosTech.Filters;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using API_PortalSantosTech.Services;
using System.Threading.RateLimiting;
using API_PortalSantosTech.Interfaces;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsProduction())
{
    builder.WebHost.UseUrls("http://0.0.0.0:8080");
}

var allowedCorsOrigins = ResolveAllowedCorsOrigins(builder.Configuration);

var connectionString =
    $"Host={Environment.GetEnvironmentVariable("DB_SERVER")};" +
    $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
    $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
    $"Username={Environment.GetEnvironmentVariable("DB_USER")};" +
    $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};" +
    $"Ssl Mode={Environment.GetEnvironmentVariable("DB_SSL")};";


builder.Services.AddCors(options =>
{
    // [SEC] restrict CORS to known frontend origin
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins(allowedCorsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddControllers();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IEmailService, SendGridEmailService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddHttpClient<AIService>();

// Hangfire
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(connectionString));

builder.Services.AddHangfireServer();

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

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu_token}"
    });

    c.OrderActionsBy(apiDesc => apiDesc.GroupName);
    c.DocumentFilter<HangfireDocumentFilter>();
    c.OperationFilter<AuthorizeOperationFilter>();
});

builder.Services.AddRateLimiter(options =>
{
    var isDevelopment = builder.Environment.IsDevelopment();

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "rate_limit_exceeded",
            message = "Muitas requisicoes em pouco tempo. Aguarde antes de tentar novamente."
        }, cancellationToken);
    };

    // [SEC] global default policy: disabled in development, enabled in shared environments
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ResolveRateLimitKey(httpContext, isDevelopment),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = isDevelopment ? 1000 : 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // [SEC] login policy: relaxed in development to avoid local lockouts
    options.AddPolicy("loginPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ResolveRateLimitKey(httpContext, isDevelopment),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = isDevelopment ? 30 : 5,
                Window = isDevelopment ? TimeSpan.FromMinutes(1) : TimeSpan.FromSeconds(30),
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

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (!string.IsNullOrWhiteSpace(context.Token))
                    return Task.CompletedTask;

                if (context.Request.Cookies.TryGetValue(TokenService.AuthCookieName, out var cookieToken))
                {
                    context.Token = cookieToken;
                }

                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

app.UseForwardedHeaders();
app.UseCors("AllowFrontend");

// [SEC] Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// [SEC] Hangfire dashboard requires admin authentication
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});
HangfireJobs.Register();

app.MapControllers();

static IResult ApiHealthResponse() => Results.Ok("API Running");

app.MapGet("/", ApiHealthResponse);

static string[] ResolveAllowedCorsOrigins(ConfigurationManager configuration)
{
    var configuredOrigins = configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();

    var legacyOrigin = configuration["Cors:AllowedOrigin"];
    if (!string.IsNullOrWhiteSpace(legacyOrigin))
    {
        configuredOrigins = configuredOrigins.Append(legacyOrigin).ToArray();
    }

    var normalizedOrigins = configuredOrigins
        .Where(origin => !string.IsNullOrWhiteSpace(origin))
        .Select(origin => origin.Trim().TrimEnd('/'))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    return normalizedOrigins.Length > 0
        ? normalizedOrigins
        : ["http://localhost:3000"];
}
app.Run();

static string ResolveRateLimitKey(HttpContext httpContext, bool isDevelopment)
{
    var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].ToString();
    var forwardedIp = string.IsNullOrWhiteSpace(forwardedFor)
        ? null
        : forwardedFor.Split(',')[0].Trim();

    var remoteIp = forwardedIp
        ?? httpContext.Connection.RemoteIpAddress?.ToString()
        ?? "unknown";

    if (!isDevelopment)
        return remoteIp;

    var path = httpContext.Request.Path.Value ?? "unknown-path";
    return $"{remoteIp}:{path}";
}
