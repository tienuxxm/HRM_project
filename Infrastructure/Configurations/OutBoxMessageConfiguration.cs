using Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class OutBoxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(m => m.Type)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(m => m.OccurredOnUtc)
            .HasDefaultValueSql("CURRENT_DATE")
            .IsRequired();
    }
}