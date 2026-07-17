using Domain.WorkCalendars;
using Domain.LeaveRequests;
using Domain.Employees;
using Domain.LeaveTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class LeaveRequestRecalculationAuditConfiguration : IEntityTypeConfiguration<LeaveRequestRecalculationAudit>
{
    public void Configure(EntityTypeBuilder<LeaveRequestRecalculationAudit> builder)
    {
        builder.ToTable("leave_request_recalculation_audit");

        builder.HasKey(lrra => lrra.Id);

        builder.Property(lrra => lrra.Id)
            .HasConversion(id => id.Value, value => new LeaveRequestRecalculationAuditId(value));

        builder.Property(lrra => lrra.BatchId)
            .HasConversion(id => id!.Value, value => new CalendarImportBatchId(value));

        builder.Property(lrra => lrra.LeaveRequestId)
            .HasConversion(id => id.Value, value => new LeaveRequestId(value))
            .IsRequired();

        builder.Property(lrra => lrra.EmployeeId)
            .HasConversion(id => id.Value, value => new EmployeeId(value))
            .IsRequired();

        builder.Property(lrra => lrra.LeaveTypeId)
            .HasConversion(id => id.Value, value => new LeaveTypeId(value))
            .IsRequired();

        builder.Property(lrra => lrra.OldStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(lrra => lrra.NewStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(lrra => lrra.OldDuration)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(lrra => lrra.NewDuration)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(lrra => lrra.OldProcessedBy);

        builder.Property(lrra => lrra.OldProcessedAt);

        builder.Property(lrra => lrra.OldComment)
            .HasMaxLength(500);

        builder.Property(lrra => lrra.RecalculatedAt)
            .IsRequired();

        builder.Property(lrra => lrra.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(lrra => lrra.ErrorMessage)
            .HasMaxLength(1000);

        // Mối quan hệ
        builder.HasOne(lrra => lrra.Batch)
            .WithMany()
            .HasForeignKey(lrra => lrra.BatchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(lrra => lrra.LeaveRequest)
            .WithMany()
            .HasForeignKey(lrra => lrra.LeaveRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lrra => lrra.Employee)
            .WithMany()
            .HasForeignKey(lrra => lrra.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lrra => lrra.LeaveType)
            .WithMany()
            .HasForeignKey(lrra => lrra.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
