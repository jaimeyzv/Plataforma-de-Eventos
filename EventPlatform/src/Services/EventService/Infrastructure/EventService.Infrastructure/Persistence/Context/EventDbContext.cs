using EventService.Infrastructure.Persistence.Configurations;
using EventService.Infrastructure.Persistence.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.Persistence.Context;

public class EventDbContext : DbContext
{
    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options)
    {
    }

    public DbSet<EventEntity> Events => Set<EventEntity>();
    public DbSet<ZoneEntity> Zones => Set<ZoneEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new EventEntityConfiguration());
        builder.ApplyConfiguration(new ZoneEntityConfiguration());

        // Transactional outbox tables (MassTransit). They are written in the same
        // transaction as the domain data, guaranteeing reliable, exactly-once publishing.
        builder.AddInboxStateEntity();
        builder.AddOutboxMessageEntity();
        builder.AddOutboxStateEntity();

        base.OnModelCreating(builder);
    }
}
