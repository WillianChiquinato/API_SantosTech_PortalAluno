using System.Text.Json;
using StackExchange.Redis;

namespace API_PortalSantosTech.Services;

public record SantosUserProfile(
    string Id,
    string Email,
    string? Username,
    string Name,
    int Role,
    string? CustomRoleId,
    string? AvatarUrl,
    string? SuspendedAt,
    Dictionary<string, List<string>>? Permissions
);

public interface ISantosAuthCacheService
{
    Task<SantosUserProfile?> GetAsync(string userId);
    Task SetAsync(SantosUserProfile user);
    Task InvalidateAsync(string userId);
}

public class SantosAuthCacheService : ISantosAuthCacheService
{
    private readonly IDatabase _db;
    private readonly TimeSpan _ttl = TimeSpan.FromSeconds(30);
    private static string CacheKey(string userId) => $"auth:session:{userId}";

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public SantosAuthCacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<SantosUserProfile?> GetAsync(string userId)
    {
        try
        {
            var raw = await _db.StringGetAsync(CacheKey(userId));
            if (raw.IsNullOrEmpty) return null;
            return JsonSerializer.Deserialize<SantosUserProfile>(raw!, _json);
        }
        catch { return null; }
    }

    public async Task SetAsync(SantosUserProfile user)
    {
        try
        {
            var json = JsonSerializer.Serialize(user, _json);
            await _db.StringSetAsync(CacheKey(user.Id), json, _ttl);
        }
        catch { }
    }

    public async Task InvalidateAsync(string userId)
    {
        try { await _db.KeyDeleteAsync(CacheKey(userId)); }
        catch { }
    }
}
