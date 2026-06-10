// Initializes the NotificationService database in MongoDB.
// Runs automatically the first time the mongo container starts
// (mounted into /docker-entrypoint-initdb.d).

const db = db.getSiblingDB("notificationdb");

// Audit log of notifications produced from integration events.
// Field names are PascalCase to match how the C# documents are persisted
// (NotificationLogDocument has no [BsonElement] overrides).
db.createCollection("notification_logs");
db.notification_logs.createIndex({ EventId: 1 });
db.notification_logs.createIndex({ CreatedAtUtc: -1 });

// Idempotency store. _id == messageId, so duplicate inserts are rejected
// automatically by the built-in (always unique) _id index — no extra index needed.
db.createCollection("processed_messages");

print("notificationdb initialized.");
