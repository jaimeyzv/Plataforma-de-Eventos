using MongoDB.Driver;
using NotificationService.Application.Abstractions;
using NotificationService.Infrastructure.Persistence.Documents;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public sealed class MongoProcessedMessageStore : IProcessedMessageStore
{
    private readonly MongoContext _context;

    public MongoProcessedMessageStore(MongoContext context) => _context = context;

    public async Task<bool> TryReserveAsync(Guid messageId, CancellationToken cancellationToken)
    {
        try
        {
            await _context.ProcessedMessages.InsertOneAsync(
                new ProcessedMessageDocument { MessageId = messageId, Status = "Reserved" },
                options: null,
                cancellationToken);
            return true;
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            // Already reserved/processed by a previous (or concurrent) delivery.
            return false;
        }
    }

    public Task ReleaseAsync(Guid messageId, CancellationToken cancellationToken) =>
        _context.ProcessedMessages.DeleteOneAsync(x => x.MessageId == messageId, cancellationToken);

    public Task MarkProcessedAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var update = Builders<ProcessedMessageDocument>.Update
            .Set(x => x.Status, "Processed")
            .Set(x => x.ProcessedAtUtc, DateTime.UtcNow);

        return _context.ProcessedMessages.UpdateOneAsync(x => x.MessageId == messageId, update, options: null, cancellationToken);
    }
}
