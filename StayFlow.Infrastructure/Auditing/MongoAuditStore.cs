using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using StayFlow.Application.Common.Auditing;

namespace StayFlow.Infrastructure.Auditing;

/// <summary>MongoDB-backed append-only audit/event store. Justifies NoSQL: schemaless, write-heavy,
/// time-ordered activity documents that are never updated.</summary>
public sealed class MongoAuditStore : IAuditStore
{
    private readonly IMongoCollection<AuditDocument> _collection;

    public MongoAuditStore(IMongoDatabase database)
    {
        _collection = database.GetCollection<AuditDocument>("audit_events");
        _collection.Indexes.CreateOne(new CreateIndexModel<AuditDocument>(
            Builders<AuditDocument>.IndexKeys.Ascending(d => d.TenantId).Descending(d => d.OccurredOnUtc)));
    }

    public Task AppendAsync(AuditRecord record, CancellationToken cancellationToken = default)
        => _collection.InsertOneAsync(AuditDocument.From(record), options: null, cancellationToken);

    public async Task<IReadOnlyList<AuditRecord>> GetRecentAsync(Guid? tenantId, int take, CancellationToken cancellationToken = default)
    {
        var filter = tenantId is { } tenant
            ? Builders<AuditDocument>.Filter.Eq(d => d.TenantId, tenant)
            : Builders<AuditDocument>.Filter.Empty;

        var documents = await _collection
            .Find(filter)
            .SortByDescending(d => d.OccurredOnUtc)
            .Limit(take)
            .ToListAsync(cancellationToken);

        return documents.Select(d => d.ToRecord()).ToList();
    }

    public sealed class AuditDocument
    {
        [BsonId]
        public Guid Id { get; set; }

        public Guid? TenantId { get; set; }

        public Guid? UserId { get; set; }

        public string EventType { get; set; } = string.Empty;

        public DateTime OccurredOnUtc { get; set; }

        public string Payload { get; set; } = string.Empty;

        public static AuditDocument From(AuditRecord record) => new()
        {
            Id = record.Id,
            TenantId = record.TenantId,
            UserId = record.UserId,
            EventType = record.EventType,
            OccurredOnUtc = record.OccurredOnUtc.UtcDateTime,
            Payload = record.Payload,
        };

        public AuditRecord ToRecord() => new(
            Id, TenantId, UserId, EventType,
            new DateTimeOffset(DateTime.SpecifyKind(OccurredOnUtc, DateTimeKind.Utc)),
            Payload);
    }
}
