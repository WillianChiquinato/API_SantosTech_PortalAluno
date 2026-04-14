using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _efDbContext;

    public NotificationRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<NotificationTemplate>> GetTemplatesAsync()
    {
        return await _efDbContext.NotificationTemplates
            .AsNoTracking()
            .OrderByDescending(template => template.UpdatedAt)
            .ThenByDescending(template => template.Id)
            .ToListAsync();
    }

    public Task<NotificationTemplate?> GetTemplateByIdAsync(int id)
    {
        return _efDbContext.NotificationTemplates
            .FirstOrDefaultAsync(template => template.Id == id);
    }

    public async Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template)
    {
        _efDbContext.NotificationTemplates.Add(template);
        await _efDbContext.SaveChangesAsync();
        return template;
    }

    public async Task<NotificationTemplate> UpdateTemplateAsync(NotificationTemplate template)
    {
        _efDbContext.NotificationTemplates.Update(template);
        await _efDbContext.SaveChangesAsync();
        return template;
    }

    public async Task<bool> DeleteTemplateAsync(NotificationTemplate template)
    {
        _efDbContext.NotificationTemplates.Remove(template);
        await _efDbContext.SaveChangesAsync();
        return true;
    }

    public Task<bool> TemplateHasRelatedDispatchesAsync(int templateId)
    {
        return _efDbContext.NotificationDispatches
            .AsNoTracking()
            .AnyAsync(dispatch => dispatch.NotificationTemplateId == templateId);
    }

    public Task<NotificationDispatch?> GetDispatchByIdAsync(int id)
    {
        return _efDbContext.NotificationDispatches
            .FirstOrDefaultAsync(dispatch => dispatch.Id == id);
    }

    public async Task<List<NotificationDispatch>> GetDispatchesAsync(int limit, int offset, string? query)
    {
        var dispatchesQuery = BuildDispatchesQuery(query);

        return await dispatchesQuery
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public Task<int> CountDispatchesAsync(string? query)
    {
        return BuildDispatchesQuery(query).CountAsync();
    }

    public async Task<bool> DeleteDispatchAsync(NotificationDispatch dispatch)
    {
        await using var transaction = await _efDbContext.Database.BeginTransactionAsync();

        var notifications = await _efDbContext.Notifications
            .Where(notification => notification.NotificationDispatchId == dispatch.Id)
            .ToListAsync();

        var dispatchRecipients = await _efDbContext.NotificationDispatchRecipients
            .Where(recipient => recipient.NotificationDispatchId == dispatch.Id)
            .ToListAsync();

        if (notifications.Count > 0)
            _efDbContext.Notifications.RemoveRange(notifications);

        if (dispatchRecipients.Count > 0)
            _efDbContext.NotificationDispatchRecipients.RemoveRange(dispatchRecipients);

        _efDbContext.NotificationDispatches.Remove(dispatch);

        await _efDbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    public async Task<List<NotificationRecipientContext>> ResolveRecipientsAsync(NotificationDispatchFiltersRequest filters)
    {
        var requestedStudentIds = (filters.StudentIds ?? [])
            .Distinct()
            .ToHashSet();
        var requestedClassIds = (filters.ClassIds ?? [])
            .Distinct()
            .ToHashSet();
        var requestedCourseIds = (filters.CourseIds ?? [])
            .Distinct()
            .ToHashSet();

        var query =
            from enrollment in _efDbContext.Enrollments.AsNoTracking()
            join user in _efDbContext.Users.AsNoTracking() on enrollment.UserId equals user.Id
            join classEntity in _efDbContext.Classes.AsNoTracking() on enrollment.ClassId equals classEntity.Id
            join course in _efDbContext.Courses.AsNoTracking() on classEntity.CourseId equals course.Id
            where user.Role == UserRole.Student
            select new NotificationRecipientContext
            {
                UserId = user.Id,
                StudentName = user.Name,
                StudentEmail = user.Email,
                ClassId = classEntity.Id,
                ClassName = classEntity.Name,
                CourseId = course.Id,
                CourseName = course.Name
            };

        if (requestedStudentIds.Count > 0 || requestedClassIds.Count > 0 || requestedCourseIds.Count > 0)
        {
            query = query.Where(row =>
                requestedStudentIds.Contains(row.UserId) ||
                requestedClassIds.Contains(row.ClassId) ||
                requestedCourseIds.Contains(row.CourseId));
        }

        var candidates = await query.ToListAsync();

        return candidates
            .GroupBy(candidate => candidate.UserId)
            .Select(group => group
                .OrderByDescending(candidate => ComputeMatchScore(candidate, requestedStudentIds, requestedClassIds, requestedCourseIds))
                .ThenBy(candidate => candidate.ClassId)
                .First())
            .ToList();
    }

    public async Task<NotificationDispatch> CreateDispatchAsync(
        NotificationDispatch dispatch,
        IEnumerable<NotificationDispatchRecipient> dispatchRecipients,
        IEnumerable<Notification> notifications)
    {
        await using var transaction = await _efDbContext.Database.BeginTransactionAsync();

        _efDbContext.NotificationDispatches.Add(dispatch);
        await _efDbContext.SaveChangesAsync();

        foreach (var dispatchRecipient in dispatchRecipients)
        {
            dispatchRecipient.NotificationDispatchId = dispatch.Id;
        }

        foreach (var notification in notifications)
        {
            notification.NotificationDispatchId = dispatch.Id;
        }

        _efDbContext.NotificationDispatchRecipients.AddRange(dispatchRecipients);
        _efDbContext.Notifications.AddRange(notifications);

        await _efDbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return dispatch;
    }

    public async Task<List<Notification>> GetInboxAsync(int userId)
    {
        return await _efDbContext.Notifications
            .AsNoTracking()
            .Include(notification => notification.NotificationTemplate)
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .ThenByDescending(notification => notification.Id)
            .ToListAsync();
    }

    public Task<int> GetUnreadCountAsync(int userId)
    {
        return _efDbContext.Notifications
            .AsNoTracking()
            .CountAsync(notification => notification.UserId == userId && notification.ReadAt == null);
    }

    public async Task<bool> MarkAsReadAsync(int userId, int notificationId)
    {
        var notification = await _efDbContext.Notifications
            .FirstOrDefaultAsync(item => item.Id == notificationId && item.UserId == userId);

        if (notification == null)
            return false;

        if (notification.ReadAt != null)
            return true;

        notification.ReadAt = DateTime.UtcNow;
        await _efDbContext.SaveChangesAsync();
        return true;
    }

    public async Task<int> MarkAllAsReadAsync(int userId)
    {
        var notifications = await _efDbContext.Notifications
            .Where(notification => notification.UserId == userId && notification.ReadAt == null)
            .ToListAsync();

        if (notifications.Count == 0)
            return 0;

        var now = DateTime.UtcNow;
        foreach (var notification in notifications)
        {
            notification.ReadAt = now;
        }

        await _efDbContext.SaveChangesAsync();
        return notifications.Count;
    }

    private static int ComputeMatchScore(
        NotificationRecipientContext candidate,
        HashSet<int> requestedStudentIds,
        HashSet<int> requestedClassIds,
        HashSet<int> requestedCourseIds)
    {
        if (requestedStudentIds.Contains(candidate.UserId))
            return 300;

        if (requestedClassIds.Contains(candidate.ClassId))
            return 200;

        if (requestedCourseIds.Contains(candidate.CourseId))
            return 100;

        return 0;
    }

    private IQueryable<NotificationDispatch> BuildDispatchesQuery(string? query)
    {
        var dispatchesQuery = _efDbContext.NotificationDispatches
            .AsNoTracking()
            .Include(dispatch => dispatch.NotificationTemplate)
            .OrderByDescending(dispatch => dispatch.CreatedAt)
            .ThenByDescending(dispatch => dispatch.Id)
            .AsQueryable();

        if (string.IsNullOrWhiteSpace(query))
            return dispatchesQuery;

        var normalizedQuery = query.Trim().ToLower();

        return dispatchesQuery.Where(dispatch =>
            (dispatch.TemplateName ?? string.Empty).ToLower().Contains(normalizedQuery) ||
            (dispatch.TriggeredByActorName ?? string.Empty).ToLower().Contains(normalizedQuery) ||
            (dispatch.TriggeredByActorEmail ?? string.Empty).ToLower().Contains(normalizedQuery));
    }
}
