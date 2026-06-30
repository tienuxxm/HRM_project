using Domain.LeaveRequests;
using Domain.Employees;
using Domain.LeaveTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("leave_request");

        builder.HasKey(lr => lr.Id);

        builder.Property(lr => lr.Id)
            .HasConversion(id => id.Value, value => new LeaveRequestId(value));

        builder.Property(lr => lr.EmployeeId)
            .HasConversion(id => id.Value, value => new EmployeeId(value))
            .IsRequired();

        builder.Property(lr => lr.LeaveTypeId)
            .HasConversion(id => id.Value, value => new LeaveTypeId(value))
            .IsRequired();

        builder.Property(lr => lr.StartDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(lr => lr.EndDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(lr => lr.StartDayPart)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(lr => lr.EndDayPart)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(lr => lr.Duration)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(lr => lr.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(lr => lr.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(lr => lr.CreatedAt)
            .IsRequired();

        builder.Property(lr => lr.ProcessedAt);

        builder.Property(lr => lr.ProcessedBy);

        builder.Property(lr => lr.Comment)
            .HasMaxLength(500);

        // Mối quan hệ
        builder.HasOne(lr => lr.Employee)
            .WithMany()
            .HasForeignKey(lr => lr.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lr => lr.LeaveType)
            .WithMany()
            .HasForeignKey(lr => lr.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index để tối ưu kiểm tra trùng lịch (Overlap check) và truy vấn
        builder.HasIndex(lr => new { lr.EmployeeId, lr.StartDate, lr.EndDate });
    }
}
