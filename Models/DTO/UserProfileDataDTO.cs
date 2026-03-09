namespace API_PortalSantosTech.Models.DTO;

public class UserProfileDataDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Bio { get; set; }
    public string? Role { get; set; }
    public string? PasswordHash { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? CoverPictureUrl { get; set; }
    public string? LevelUser { get; set; }
    public float PointsQuantity { get; set; }

    public ClassDTO? Class { get; set; }
    public List<BadgeDTO>? StudentBadges { get; set; }
}

public class ClassDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class BadgeDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? IconURL { get; set; }
}

public class PasswordRecoveryResult
{
    public bool Success { get; set; }
    public string? PasswordRecoveryCode { get; set; }
}

public class UserWithAbilityDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public List<string>? Abilities { get; set; }
}