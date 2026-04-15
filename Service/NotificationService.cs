using System.Text.Json;
using System.Text.RegularExpressions;
using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class NotificationService : INotificationService
{
    private static readonly Regex PlaceholderRegex = new("{{\\s*([a-zA-Z0-9._]+)\\s*}}", RegexOptions.Compiled);
    private static readonly HashSet<string> AllowedPlaceholders = new(StringComparer.OrdinalIgnoreCase)
    {
        "aluno.nome",
        "aluno.email",
        "turma.nome",
        "curso.nome"
    };

    private readonly ILogger<NotificationService> _logger;
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(ILogger<NotificationService> logger, INotificationRepository notificationRepository)
    {
        _logger = logger;
        _notificationRepository = notificationRepository;
    }

    public async Task<CustomResponse<IEnumerable<NotificationTemplateSummaryDTO>>> GetTemplatesAsync()
    {
        try
        {
            var templates = await _notificationRepository.GetTemplatesAsync();
            return CustomResponse<IEnumerable<NotificationTemplateSummaryDTO>>.SuccessTrade(
                templates.Select(MapTemplate));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error in GetTemplatesAsync");
            return CustomResponse<IEnumerable<NotificationTemplateSummaryDTO>>.Fail("Ocorreu um erro ao listar os templates.");
        }
    }

    public async Task<CustomResponse<NotificationTemplateSummaryDTO>> CreateTemplateAsync(NotificationTemplateUpsertRequest request)
    {
        try
        {
            var validationError = ValidateTemplateRequest(request);
            if (validationError != null)
                return CustomResponse<NotificationTemplateSummaryDTO>.Fail(validationError);

            var now = DateTime.UtcNow;
            var template = new NotificationTemplate
            {
                Name = request.Name.Trim(),
                TitleTemplate = request.TitleTemplate.Trim(),
                MessageTemplate = request.MessageTemplate.Trim(),
                IsActive = request.IsActive,
                CreatedByActorId = TrimOrNull(request.Actor?.ExternalId),
                CreatedByActorName = TrimOrNull(request.Actor?.Name),
                CreatedByActorEmail = TrimOrNull(request.Actor?.Email),
                UpdatedByActorId = TrimOrNull(request.Actor?.ExternalId),
                UpdatedByActorName = TrimOrNull(request.Actor?.Name),
                UpdatedByActorEmail = TrimOrNull(request.Actor?.Email),
                CreatedAt = now,
                UpdatedAt = now
            };

            var createdTemplate = await _notificationRepository.CreateTemplateAsync(template);
            return CustomResponse<NotificationTemplateSummaryDTO>.SuccessTrade(MapTemplate(createdTemplate));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error in CreateTemplateAsync");
            return CustomResponse<NotificationTemplateSummaryDTO>.Fail("Ocorreu um erro ao criar o template.");
        }
    }

    public async Task<CustomResponse<NotificationTemplateSummaryDTO>> UpdateTemplateAsync(int id, NotificationTemplateUpsertRequest request)
    {
        try
        {
            var validationError = ValidateTemplateRequest(request);
            if (validationError != null)
                return CustomResponse<NotificationTemplateSummaryDTO>.Fail(validationError);

            var template = await _notificationRepository.GetTemplateByIdAsync(id);
            if (template == null)
                return CustomResponse<NotificationTemplateSummaryDTO>.Fail("Template não encontrado.");

            template.Name = request.Name.Trim();
            template.TitleTemplate = request.TitleTemplate.Trim();
            template.MessageTemplate = request.MessageTemplate.Trim();
            template.IsActive = request.IsActive;
            template.UpdatedByActorId = TrimOrNull(request.Actor?.ExternalId);
            template.UpdatedByActorName = TrimOrNull(request.Actor?.Name);
            template.UpdatedByActorEmail = TrimOrNull(request.Actor?.Email);
            template.UpdatedAt = DateTime.UtcNow;

            var updatedTemplate = await _notificationRepository.UpdateTemplateAsync(template);
            return CustomResponse<NotificationTemplateSummaryDTO>.SuccessTrade(MapTemplate(updatedTemplate));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error in UpdateTemplateAsync");
            return CustomResponse<NotificationTemplateSummaryDTO>.Fail("Ocorreu um erro ao atualizar o template.");
        }
    }

    public async Task<CustomResponse<bool>> DeleteTemplateAsync(int id)
    {
        try
        {
            var template = await _notificationRepository.GetTemplateByIdAsync(id);
            if (template == null)
                return CustomResponse<bool>.Fail("Template não encontrado.");

            var hasRelatedDispatches = await _notificationRepository.TemplateHasRelatedDispatchesAsync(id);
            if (hasRelatedDispatches)
                return CustomResponse<bool>.Fail("Exclua os disparos desse template antes de removê-lo.");

            await _notificationRepository.DeleteTemplateAsync(template);
            return CustomResponse<bool>.SuccessTrade(true);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error in DeleteTemplateAsync");
            return CustomResponse<bool>.Fail("Ocorreu um erro ao excluir o template.");
        }
    }

    public async Task<CustomResponse<IEnumerable<NotificationDispatchSummaryDTO>>> GetDispatchesAsync(int limit, int offset, string? query)
    {
        try
        {
            var dispatches = await _notificationRepository.GetDispatchesAsync(limit, offset, query);
            var totalRows = await _notificationRepository.CountDispatchesAsync(query);
            var result = dispatches.Select(dispatch => new NotificationDispatchSummaryDTO
            {
                Id = dispatch.Id,
                NotificationTemplateId = dispatch.NotificationTemplateId,
                TemplateName = dispatch.TemplateName,
                TriggeredByActorName = dispatch.TriggeredByActorName,
                TriggeredByActorEmail = dispatch.TriggeredByActorEmail,
                Filters = DeserializeFilters(dispatch.FiltersJson),
                TotalRecipients = dispatch.TotalRecipients,
                FailedRecipients = dispatch.FailedRecipients,
                CreatedAt = dispatch.CreatedAt
            });

            return CustomResponse<IEnumerable<NotificationDispatchSummaryDTO>>.SuccessTrade(result, totalRows);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error in GetDispatchesAsync");
            return CustomResponse<IEnumerable<NotificationDispatchSummaryDTO>>.Fail("Ocorreu um erro ao listar os disparos.");
        }
    }

    public async Task<CustomResponse<bool>> DeleteDispatchAsync(int id)
    {
        try
        {
            var dispatch = await _notificationRepository.GetDispatchByIdAsync(id);
            if (dispatch == null)
                return CustomResponse<bool>.Fail("Disparo não encontrado.");

            await _notificationRepository.DeleteDispatchAsync(dispatch);
            return CustomResponse<bool>.SuccessTrade(true);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error in DeleteDispatchAsync");
            return CustomResponse<bool>.Fail("Ocorreu um erro ao excluir o disparo.");
        }
    }

    public async Task<CustomResponse<NotificationDispatchResultDTO>> DispatchTemplateAsync(int templateId, NotificationDispatchRequest request)
    {
        try
        {
            var filters = NormalizeFilters(request.Filters);
            if (filters.CourseIds!.Count == 0 && filters.ClassIds!.Count == 0 && filters.StudentIds!.Count == 0)
                return CustomResponse<NotificationDispatchResultDTO>.Fail("Selecione ao menos um filtro de destinatário.");

            var template = await _notificationRepository.GetTemplateByIdAsync(templateId);
            if (template == null)
                return CustomResponse<NotificationDispatchResultDTO>.Fail("Template não encontrado.");

            if (!template.IsActive)
                return CustomResponse<NotificationDispatchResultDTO>.Fail("O template está inativo.");

            var validationError = ValidateTemplateContent(template.TitleTemplate, template.MessageTemplate);
            if (validationError != null)
                return CustomResponse<NotificationDispatchResultDTO>.Fail(validationError);

            var recipients = await _notificationRepository.ResolveRecipientsAsync(filters);
            if (recipients.Count == 0)
                return CustomResponse<NotificationDispatchResultDTO>.Fail("Nenhum destinatário foi encontrado para os filtros selecionados.");

            var eligibleRecipients = recipients
                .Where(recipient =>
                    recipient.ClassId.HasValue &&
                    recipient.CourseId.HasValue &&
                    !string.IsNullOrWhiteSpace(recipient.ClassName) &&
                    !string.IsNullOrWhiteSpace(recipient.CourseName))
                .ToList();

            if (eligibleRecipients.Count == 0)
                return CustomResponse<NotificationDispatchResultDTO>.Fail("Nenhum destinatário com turma e curso vinculados foi encontrado para os filtros selecionados.");

            var now = DateTime.UtcNow;
            var failedRecipients = BuildFailedRecipients(filters, recipients, eligibleRecipients, now);
            var dispatchRecipients = new List<NotificationDispatchRecipient>();
            var notifications = new List<Notification>();

            foreach (var recipient in eligibleRecipients)
            {
                var rendered = RenderTemplate(template, recipient);
                var metadataJson = JsonSerializer.Serialize(new NotificationMetadataDTO
                {
                    TemplateName = template.Name,
                    ClassName = recipient.ClassName,
                    CourseName = recipient.CourseName
                });

                dispatchRecipients.Add(new NotificationDispatchRecipient
                {
                    RecipientUserId = recipient.UserId,
                    RecipientName = recipient.RecipientName,
                    RecipientEmail = recipient.RecipientEmail,
                    ClassName = recipient.ClassName,
                    CourseName = recipient.CourseName,
                    Title = rendered.Title,
                    Message = rendered.Message,
                    Status = "delivered",
                    CreatedAt = now
                });

                notifications.Add(new Notification
                {
                    UserId = recipient.UserId,
                    NotificationTemplateId = template.Id,
                    Title = rendered.Title,
                    Message = rendered.Message,
                    MetadataJson = metadataJson,
                    CreatedAt = now
                });
            }

            dispatchRecipients.AddRange(failedRecipients);

            var dispatch = new NotificationDispatch
            {
                NotificationTemplateId = template.Id,
                TemplateName = template.Name,
                TriggeredByActorId = TrimOrNull(request.Actor?.ExternalId),
                TriggeredByActorName = TrimOrNull(request.Actor?.Name),
                TriggeredByActorEmail = TrimOrNull(request.Actor?.Email),
                FiltersJson = JsonSerializer.Serialize(filters),
                TotalRecipients = notifications.Count,
                FailedRecipients = failedRecipients.Count,
                CreatedAt = now
            };

            var createdDispatch = await _notificationRepository.CreateDispatchAsync(dispatch, dispatchRecipients, notifications);

            return CustomResponse<NotificationDispatchResultDTO>.SuccessTrade(new NotificationDispatchResultDTO
            {
                DispatchId = createdDispatch.Id,
                NotificationTemplateId = template.Id,
                TemplateName = template.Name,
                TotalRecipients = notifications.Count,
                FailedRecipients = failedRecipients.Count,
                CreatedAt = createdDispatch.CreatedAt
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error in DispatchTemplateAsync");
            return CustomResponse<NotificationDispatchResultDTO>.Fail("Ocorreu um erro ao disparar a notificação.");
        }
    }

    public async Task<CustomResponse<IEnumerable<NotificationInboxItemDTO>>> GetInboxAsync(int userId)
    {
        try
        {
            var notifications = await _notificationRepository.GetInboxAsync(userId);
            var result = notifications.Select(notification =>
            {
                var metadata = DeserializeMetadata(notification.MetadataJson);
                return new NotificationInboxItemDTO
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    TemplateName = metadata.TemplateName ?? notification.NotificationTemplate?.Name,
                    ClassName = metadata.ClassName,
                    CourseName = metadata.CourseName,
                    CreatedAt = notification.CreatedAt,
                    ReadAt = notification.ReadAt
                };
            });

            return CustomResponse<IEnumerable<NotificationInboxItemDTO>>.SuccessTrade(result);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error in GetInboxAsync");
            return CustomResponse<IEnumerable<NotificationInboxItemDTO>>.Fail("Ocorreu um erro ao listar as notificações.");
        }
    }

    public async Task<CustomResponse<NotificationUnreadCountDTO>> GetUnreadCountAsync(int userId)
    {
        try
        {
            var count = await _notificationRepository.GetUnreadCountAsync(userId);
            return CustomResponse<NotificationUnreadCountDTO>.SuccessTrade(new NotificationUnreadCountDTO
            {
                Count = count
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error in GetUnreadCountAsync");
            return CustomResponse<NotificationUnreadCountDTO>.Fail("Ocorreu um erro ao carregar o contador de notificações.");
        }
    }

    public async Task<CustomResponse<bool>> MarkAsReadAsync(int userId, int notificationId)
    {
        try
        {
            var marked = await _notificationRepository.MarkAsReadAsync(userId, notificationId);
            return marked
                ? CustomResponse<bool>.SuccessTrade(true)
                : CustomResponse<bool>.Fail("Notificação não encontrada.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error in MarkAsReadAsync");
            return CustomResponse<bool>.Fail("Ocorreu um erro ao marcar a notificação como lida.");
        }
    }

    public async Task<CustomResponse<int>> MarkAllAsReadAsync(int userId)
    {
        try
        {
            var count = await _notificationRepository.MarkAllAsReadAsync(userId);
            return CustomResponse<int>.SuccessTrade(count);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error in MarkAllAsReadAsync");
            return CustomResponse<int>.Fail("Ocorreu um erro ao marcar as notificações como lidas.");
        }
    }

    private static NotificationTemplateSummaryDTO MapTemplate(NotificationTemplate template)
    {
        return new NotificationTemplateSummaryDTO
        {
            Id = template.Id,
            Name = template.Name,
            TitleTemplate = template.TitleTemplate,
            MessageTemplate = template.MessageTemplate,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }

    private static NotificationDispatchFiltersRequest NormalizeFilters(NotificationDispatchFiltersRequest? filters)
    {
        return new NotificationDispatchFiltersRequest
        {
            CourseIds = NormalizeIdList(filters?.CourseIds),
            ClassIds = NormalizeIdList(filters?.ClassIds),
            StudentIds = NormalizeIdList(filters?.StudentIds)
        };
    }

    private static List<int> NormalizeIdList(List<int>? ids)
    {
        return (ids ?? [])
            .Where(id => id > 0)
            .Distinct()
            .OrderBy(id => id)
            .ToList();
    }

    private static string? ValidateTemplateRequest(NotificationTemplateUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return "Informe o nome interno do template.";

        if (string.IsNullOrWhiteSpace(request.TitleTemplate))
            return "Informe o título do template.";

        if (string.IsNullOrWhiteSpace(request.MessageTemplate))
            return "Informe a mensagem do template.";

        return ValidateTemplateContent(request.TitleTemplate, request.MessageTemplate);
    }

    private static string? ValidateTemplateContent(string titleTemplate, string messageTemplate)
    {
        var placeholders = ExtractPlaceholders(titleTemplate)
            .Concat(ExtractPlaceholders(messageTemplate))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var invalidPlaceholders = placeholders
            .Where(placeholder => !AllowedPlaceholders.Contains(placeholder))
            .OrderBy(placeholder => placeholder)
            .ToList();

        if (invalidPlaceholders.Count == 0)
            return null;

        return $"Placeholder inválido: {string.Join(", ", invalidPlaceholders)}.";
    }

    private static IEnumerable<string> ExtractPlaceholders(string content)
    {
        return PlaceholderRegex.Matches(content)
            .Select(match => match.Groups[1].Value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value));
    }

    private static NotificationTemplateRenderResult RenderTemplate(NotificationTemplate template, NotificationRecipientContext recipient)
    {
        return new NotificationTemplateRenderResult
        {
            Title = ReplacePlaceholders(template.TitleTemplate, recipient),
            Message = ReplacePlaceholders(template.MessageTemplate, recipient)
        };
    }

    private static string ReplacePlaceholders(string template, NotificationRecipientContext recipient)
    {
        return PlaceholderRegex.Replace(template, match =>
        {
            var placeholder = match.Groups[1].Value.Trim();
            return placeholder switch
            {
                "aluno.nome" => recipient.RecipientName ?? string.Empty,
                "aluno.email" => recipient.RecipientEmail ?? string.Empty,
                "turma.nome" => recipient.ClassName ?? string.Empty,
                "curso.nome" => recipient.CourseName ?? string.Empty,
                _ => match.Value
            };
        });
    }

    private static List<NotificationDispatchRecipient> BuildFailedRecipients(
        NotificationDispatchFiltersRequest filters,
        IEnumerable<NotificationRecipientContext> recipients,
        IEnumerable<NotificationRecipientContext> eligibleRecipients,
        DateTime now)
    {
        var deliveredStudentIds = eligibleRecipients
            .Select(recipient => recipient.UserId)
            .ToHashSet();

        var failedRecipients = (filters.StudentIds ?? [])
            .Where(studentId => !deliveredStudentIds.Contains(studentId))
            .Select(studentId => new NotificationDispatchRecipient
            {
                RecipientUserId = studentId,
                Status = "failed",
                FailureReason = "Aluno não encontrado no portal do Aluno ou sem matrícula ativa.",
                CreatedAt = now
            })
            .ToList();

        failedRecipients.AddRange(recipients
            .Where(recipient =>
                !deliveredStudentIds.Contains(recipient.UserId) &&
                (!recipient.ClassId.HasValue ||
                 !recipient.CourseId.HasValue ||
                 string.IsNullOrWhiteSpace(recipient.ClassName) ||
                 string.IsNullOrWhiteSpace(recipient.CourseName)))
            .Select(recipient => new NotificationDispatchRecipient
            {
                RecipientUserId = recipient.UserId,
                RecipientName = recipient.RecipientName,
                RecipientEmail = recipient.RecipientEmail,
                Status = "failed",
                FailureReason = "Usuario sem turma ou curso vinculado.",
                CreatedAt = now
            }));

        return failedRecipients
            .GroupBy(recipient => recipient.RecipientUserId)
            .Select(group => group.First())
            .ToList();
    }

    private static NotificationDispatchFiltersRequest DeserializeFilters(string filtersJson)
    {
        if (string.IsNullOrWhiteSpace(filtersJson))
            return new NotificationDispatchFiltersRequest();

        try
        {
            return JsonSerializer.Deserialize<NotificationDispatchFiltersRequest>(filtersJson) ?? new NotificationDispatchFiltersRequest();
        }
        catch
        {
            return new NotificationDispatchFiltersRequest();
        }
    }

    private static NotificationMetadataDTO DeserializeMetadata(string metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
            return new NotificationMetadataDTO();

        try
        {
            return JsonSerializer.Deserialize<NotificationMetadataDTO>(metadataJson) ?? new NotificationMetadataDTO();
        }
        catch
        {
            return new NotificationMetadataDTO();
        }
    }

    private static string? TrimOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }

    private class NotificationMetadataDTO
    {
        public string? TemplateName { get; set; }
        public string? ClassName { get; set; }
        public string? CourseName { get; set; }
    }
}



