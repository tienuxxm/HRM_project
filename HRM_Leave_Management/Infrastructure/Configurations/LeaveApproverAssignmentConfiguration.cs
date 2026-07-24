using Domain.LeaveApproverAssignments;
using Domain.Employees;
using Domain.Departments;
using Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class LeaveApproverAssignmentConfiguration : IEntityTypeConfiguration<LeaveApproverAssignment>
{
    public void Configure(EntityTypeBuilder<LeaveApproverAssignment> builder)
    {
        builder.ToTable("leave_approver_assignment");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => new LeaveApproverAssignmentId(value));

        builder.Property(a => a.ApproverEmployeeId)
            .HasConversion(id => id.Value, value => new EmployeeId(value))
            .IsRequired();

        builder.Property(a => a.TargetDepartmentId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new DepartmentId(value.Value) : null);

        builder.Property(a => a.TargetPositionId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new PositionId(value.Value) : null);

        builder.Property(a => a.IsActive)
            .IsRequired();

        builder.Property(a => a.EffectiveFrom)
            .HasColumnType("date");

        builder.Property(a => a.EffectiveTo)
            .HasColumnType("date");

        builder.Property(a => a.CreatedDate)
            .IsRequired();

        // Relationships
        builder.HasOne(a => a.Approver)
            .WithMany()
            .HasForeignKey(a => a.ApproverEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.TargetDepartment)
            .WithMany()
            .HasForeignKey(a => a.TargetDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.TargetPosition)
            .WithMany()
            .HasForeignKey(a => a.TargetPositionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_leave_approver_assignment_positions_target_position_temp_id");

        // Indexes
        builder.HasIndex(a => new { a.ApproverEmployeeId, a.TargetDepartmentId, a.TargetPositionId });
    }
}
