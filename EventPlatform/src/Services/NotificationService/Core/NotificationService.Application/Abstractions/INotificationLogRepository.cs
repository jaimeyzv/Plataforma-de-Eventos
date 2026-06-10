using NotificationService.Domain.Entities;

namespace NotificationService.Application.Abstractions;

public interface INotificationLogRepository
{
    Task AddAsync(NotificationLog log, CancellationToken cancellationToken);

    Task<IReadOnlyList<NotificationLog>> GetRecentAsync(int limit, CancellationToken cancellationToken);
}
