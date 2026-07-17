using Domain.WorkCalendars;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class WorkCalendarDayConfiguration : IEntityTypeConfiguration<WorkCalendarDay>
{
    public void Configure(EntityTypeBuilder<WorkCalendarDay> builder)
    {
        builder.ToTable("work_calendar_day");

        builder.HasKey(wcd => wcd.Id);

        builder.Property(wcd => wcd.Id)
            .HasConversion(id => id.Value, value => new WorkCalendarDayId(value));

        builder.Property(wcd => wcd.Date)
            .IsRequired();

        builder.HasIndex(wcd => wcd.Date)
            .IsUnique();

        builder.Property(wcd => wcd.DayType)
            .IsRequired();

        builder.Property(wcd => wcd.WorkShift)
            .IsRequired();

        builder.Property(wcd => wcd.Description)
            .HasMaxLength(500);

        builder.Property(wcd => wcd.IsActive)
            .IsRequired();

        builder.Property(wcd => wcd.CreatedBy)
            .IsRequired();

        builder.Property(wcd => wcd.CreatedAt)
            .IsRequired();
    }
}
