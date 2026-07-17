using Domain.WorkCalendars;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class CalendarImportBatchConfiguration : IEntityTypeConfiguration<CalendarImportBatch>
{
    public void Configure(EntityTypeBuilder<CalendarImportBatch> builder)
    {
        builder.ToTable("calendar_import_batch");

        builder.HasKey(cib => cib.Id);

        builder.Property(cib => cib.Id)
            .HasConversion(id => id.Value, value => new CalendarImportBatchId(value));

        builder.Property(cib => cib.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(cib => cib.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(cib => cib.CreatedBy)
            .IsRequired();

        builder.Property(cib => cib.CreatedAt)
            .IsRequired();

        builder.Property(cib => cib.ProcessedBy);

        builder.Property(cib => cib.ProcessedAt);
    }
}
