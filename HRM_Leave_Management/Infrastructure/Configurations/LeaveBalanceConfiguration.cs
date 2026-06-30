using Domain.LeaveBalances;
using Domain.Employees;
using Domain.LeaveTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("leave_balance");

        builder.HasKey(lb => lb.Id);

        builder.Property(lb => lb.Id)
            .HasConversion(id => id.Value, value => new LeaveBalanceId(value));

        builder.Property(lb => lb.EmployeeId)
            .HasConversion(id => id.Value, value => new EmployeeId(value))
            .IsRequired();

        builder.Property(lb => lb.LeaveTypeId)
            .HasConversion(id => id.Value, value => new LeaveTypeId(value))
            .IsRequired();

        builder.Property(lb => lb.Year)
            .IsRequired();

        builder.Property(lb => lb.AllocatedDays)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(lb => lb.UsedDays)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(lb => lb.IsActive)
            .IsRequired();

        builder.Property(lb => lb.CreatedDate)
            .IsRequired();

        // Mối quan hệ
        builder.HasOne(lb => lb.Employee)
            .WithMany()
            .HasForeignKey(lb => lb.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lb => lb.LeaveType)
            .WithMany()
            .HasForeignKey(lb => lb.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Partial unique index
        builder.HasIndex(lb => new { lb.EmployeeId, lb.LeaveTypeId, lb.Year })
            .IsUnique()
            .HasFilter("is_active = true")
            .HasDatabaseName("ix_leave_balance_unique_active");
    }
}
