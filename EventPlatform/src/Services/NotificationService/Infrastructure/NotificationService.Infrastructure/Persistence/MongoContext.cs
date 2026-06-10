using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NotificationService.Infrastructure.Persistence.Documents;

namespace NotificationService.Infrastructure.Persistence;

/// <summary>
/// Owns the MongoDB connection and exposes typed collections. Ensures the unique index
/// on the processed-messages collection that backs idempotency.
/// </summary>
public sealed class MongoContext
{
    private readonly MongoSettings _settings;

    public IMongoCollection<NotificationLogDocument> Notifications { get; }
    public IMongoCollection<ProcessedMessageDocument> ProcessedMessages { get; }

    public MongoContext(IMongoClient client, IOptions<MongoSettings> options)
    {
        _settings = options.Value;
        var database = client.GetDatabase(_settings.Database);

        Notifications = database.GetCollection<NotificationLogDocument>(_settings.NotificationsCollection);
        ProcessedMessages = database.GetCollection<ProcessedMessageDocument>(_settings.ProcessedMessagesCollection);

        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        // _id is already unique; this index speeds up lookups by EventId on the audit log.
        var byEvent = new CreateIndexModel<NotificationLogDocument>(
            Builders<NotificationLogDocument>.IndexKeys.Ascending(x => x.EventId));
        Notifications.Indexes.CreateOne(byEvent);
    }
}
