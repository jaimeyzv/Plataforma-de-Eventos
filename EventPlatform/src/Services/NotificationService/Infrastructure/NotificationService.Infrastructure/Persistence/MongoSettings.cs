namespace NotificationService.Infrastructure.Persistence;

public sealed class MongoSettings
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "notificationdb";
    public string NotificationsCollection { get; set; } = "notification_logs";
    public string ProcessedMessagesCollection { get; set; } = "processed_messages";
}
