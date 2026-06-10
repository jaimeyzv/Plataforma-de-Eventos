using EventService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventService.Infrastructure.Persistence.Configurations;

public class ZoneEntityConfiguration : IEntityTypeConfiguration<ZoneEntity>
{
    public void Configure(EntityTypeBuilder<ZoneEntity> builder)
    {
        builder.ToTable("Zones");

        builder.HasKey(x => x.ZoneId);

        builder.Property(x => x.ZoneId)
            .HasColumnName("ZoneId")
            .ValueGeneratedNever();

        builder.Property(x => x.EventId)
            .HasColumnName("EventId")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("Name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Price)
            .HasColumnName("Price")
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(x => x.Capacity)
            .HasColumnName("Capacity")
            .IsRequired();

        builder.HasIndex(x => x.EventId);
    }
}
