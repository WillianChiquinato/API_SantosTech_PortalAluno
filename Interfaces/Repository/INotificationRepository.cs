using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface INotificationRepository
{
    Task<List<NotificationTemplate>> GetTemplatesAsync();
    Task<NotificationTemplate?> GetTemplateByIdAsync(int id);
    Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template);
    Task<NotificationTemplate> UpdateTemplateAsync(NotificationTemplate template);
    Task<bool> DeleteTemplateAsync(NotificationTemplate template);
    Task<bool> TemplateHasRelatedDispatchesAsync(int templateId);
    Task<NotificationDispatch?> GetDispatchByIdAsync(int id);
    Task<List<NotificationDispatch>> GetDispatchesAsync(int limit, int offset, string? query);
    Task<int> CountDispatchesAsync(string? query);
    Task<bool> DeleteDispatchAsync(NotificationDispatch dispatch);
    Task<List<NotificationRecipientContext>> ResolveRecipientsAsync(NotificationDispatchFiltersRequest filters);
    Task<NotificationDispatch> CreateDispatchAsync(
        NotificationDispatch dispatch,
        IEnumerable<NotificationDispatchRecipient> dispatchRecipients,
        IEnumerable<Notification> notifications);
    Task<List<Notification>> GetInboxAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int userId, int notificationId);
    Task<int> MarkAllAsReadAsync(int userId);
}
