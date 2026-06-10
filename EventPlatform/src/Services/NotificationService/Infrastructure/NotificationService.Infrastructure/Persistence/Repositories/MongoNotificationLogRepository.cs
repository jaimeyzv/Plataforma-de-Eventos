using MongoDB.Driver;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Types;
using NotificationService.Infrastructure.Persistence.Documents;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public sealed class MongoNotificationLogRepository : INotificationLogRepository
{
    private readonly MongoContext _context;

    public MongoNotificationLogRepository(MongoContext context) => _context = context;

    public Task AddAsync(NotificationLog log, CancellationToken cancellationToken)
    {
        var document = new NotificationLogDocument
        {
            Id = log.Id,
            MessageId = log.MessageId,
            EventId = log.EventId,
            CorrelationId = log.CorrelationId,
            Channel = log.Channel,
            Recipient = log.Recipient,
            Subject = log.Subject,
            Body = log.Body,
            Status = log.Status.ToString(),
            OccurredAtUtc = log.OccurredAt.UtcDateTime,
            CreatedAtUtc = log.CreatedAt.UtcDateTime,
            Version = log.Version
        };

        return _context.Notifications.InsertOneAsync(document, options: null, cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationLog>> GetRecentAsync(int limit, CancellationToken cancellationToken)
    {
        var documents = await _context.Notifications
            .Find(FilterDefinition<NotificationLogDocument>.Empty)
            .SortByDescending(x => x.CreatedAtUtc)
            .Limit(limit)
            .ToListAsync(cancellationToken);

        return documents.Select(Map).ToList();
    }

    private static NotificationLog Map(NotificationLogDocument d) => new()
    {
        Id = d.Id,
        MessageId = d.MessageId,
        EventId = d.EventId,
        CorrelationId = d.CorrelationId,
        Channel = d.Channel,
        Recipient = d.Recipient,
        Subject = d.Subject,
        Body = d.Body,
        Status = Enum.TryParse<NotificationStatus>(d.Status, out var s) ? s : NotificationStatus.Pending,
        OccurredAt = d.OccurredAtUtc,
        CreatedAt = d.CreatedAtUtc,
        Version = d.Version
    };
}
