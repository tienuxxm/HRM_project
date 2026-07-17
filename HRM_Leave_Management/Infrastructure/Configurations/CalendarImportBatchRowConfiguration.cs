using Domain.WorkCalendars;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class CalendarImportBatchRowConfiguration : IEntityTypeConfiguration<CalendarImportBatchRow>
{
    public void Configure(EntityTypeBuilder<CalendarImportBatchRow> builder)
    {
        builder.ToTable("calendar_import_batch_row");

        builder.HasKey(cibr => cibr.Id);

        builder.Property(cibr => cibr.Id)
            .HasConversion(id => id.Value, value => new CalendarImportBatchRowId(value));

        builder.Property(cibr => cibr.BatchId)
            .HasConversion(id => id.Value, value => new CalendarImportBatchId(value))
            .IsRequired();

        builder.Property(cibr => cibr.RowIndex)
            .IsRequired();

        builder.Property(cibr => cibr.Date)
            .HasColumnType("date");

        builder.Property(cibr => cibr.DayType)
            .HasConversion<int>();

        builder.Property(cibr => cibr.WorkShift)
            .HasConversion<int>();

        builder.Property(cibr => cibr.Description)
            .HasMaxLength(500);

        builder.Property(cibr => cibr.IsActive)
            .IsRequired();

        builder.Property(cibr => cibr.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(cibr => cibr.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(cibr => cibr.RawDate)
            .HasMaxLength(100);

        builder.Property(cibr => cibr.RawDayType)
            .HasMaxLength(100);

        builder.Property(cibr => cibr.RawWorkShift)
            .HasMaxLength(100);

        // Mối quan hệ
        builder.HasOne(cibr => cibr.Batch)
            .WithMany(cib => cib.Rows)
            .HasForeignKey(cibr => cibr.BatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
