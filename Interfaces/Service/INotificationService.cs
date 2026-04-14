using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface INotificationService
{
    Task<CustomResponse<IEnumerable<NotificationTemplateSummaryDTO>>> GetTemplatesAsync();
    Task<CustomResponse<NotificationTemplateSummaryDTO>> CreateTemplateAsync(NotificationTemplateUpsertRequest request);
    Task<CustomResponse<NotificationTemplateSummaryDTO>> UpdateTemplateAsync(int id, NotificationTemplateUpsertRequest request);
    Task<CustomResponse<bool>> DeleteTemplateAsync(int id);
    Task<CustomResponse<IEnumerable<NotificationDispatchSummaryDTO>>> GetDispatchesAsync(int limit, int offset, string? query);
    Task<CustomResponse<bool>> DeleteDispatchAsync(int id);
    Task<CustomResponse<NotificationDispatchResultDTO>> DispatchTemplateAsync(int templateId, NotificationDispatchRequest request);
    Task<CustomResponse<IEnumerable<NotificationInboxItemDTO>>> GetInboxAsync(int userId);
    Task<CustomResponse<NotificationUnreadCountDTO>> GetUnreadCountAsync(int userId);
    Task<CustomResponse<bool>> MarkAsReadAsync(int userId, int notificationId);
    Task<CustomResponse<int>> MarkAllAsReadAsync(int userId);
}
