using EventService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventService.Infrastructure.Persistence.Configurations;

public class EventEntityConfiguration : IEntityTypeConfiguration<EventEntity>
{
    public void Configure(EntityTypeBuilder<EventEntity> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(x => x.EventId);

        builder.Property(x => x.EventId)
            .HasColumnName("EventId")
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasColumnName("Name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.EventDate)
            .HasColumnName("EventDate")
            .IsRequired();

        builder.Property(x => x.Place)
            .HasColumnName("Place")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Status)
            .HasColumnName("Status")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.RowVersion)
            .HasColumnName("RowVersion")
            .IsRowVersion();

        builder.HasMany(x => x.Zones)
            .WithOne(z => z.Event)
            .HasForeignKey(z => z.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.EventDate);
    }
}
