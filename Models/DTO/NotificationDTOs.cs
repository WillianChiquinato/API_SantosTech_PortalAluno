namespace API_PortalSantosTech.Models.DTO;

public class NotificationActorDTO
{
    public string? ExternalId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
}

public class NotificationTemplateUpsertRequest
{
    public string Name { get; set; } = string.Empty;
    public string TitleTemplate { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public NotificationActorDTO? Actor { get; set; }
}

public class NotificationDispatchFiltersRequest
{
    public List<int>? CourseIds { get; set; }
    public List<int>? ClassIds { get; set; }
    public List<int>? StudentIds { get; set; }
}

public class NotificationDispatchRequest
{
    public NotificationDispatchFiltersRequest Filters { get; set; } = new();
    public NotificationActorDTO? Actor { get; set; }
}

public class NotificationTemplateSummaryDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TitleTemplate { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class NotificationDispatchSummaryDTO
{
    public int Id { get; set; }
    public int NotificationTemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string? TriggeredByActorName { get; set; }
    public string? TriggeredByActorEmail { get; set; }
    public NotificationDispatchFiltersRequest Filters { get; set; } = new();
    public int TotalRecipients { get; set; }
    public int FailedRecipients { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationDispatchResultDTO
{
    public int DispatchId { get; set; }
    public int NotificationTemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public int TotalRecipients { get; set; }
    public int FailedRecipients { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationInboxItemDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TemplateName { get; set; }
    public string? ClassName { get; set; }
    public string? CourseName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsRead => ReadAt.HasValue;
}

public class NotificationUnreadCountDTO
{
    public int Count { get; set; }
}

public class NotificationMarkAsReadRequest
{
    public int NotificationId { get; set; }
}

public class NotificationRecipientContext
{
    public int UserId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentEmail { get; set; }
    public int ClassId { get; set; }
    public string? ClassName { get; set; }
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
}

internal class NotificationTemplateRenderResult
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
