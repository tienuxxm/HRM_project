using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.LeaveRequests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class ApprovalRouteAuditLogConfiguration : IEntityTypeConfiguration<ApprovalRouteAuditLog>
{
    public void Configure(EntityTypeBuilder<ApprovalRouteAuditLog> builder)
    {
        builder.ToTable("approval_route_audit_log");

        builder.HasKey(log => log.Id);

        builder.Property(log => log.Id)
            .HasConversion(id => id.Value, value => new ApprovalRouteAuditLogId(value));

        builder.Property(log => log.LeaveRequestId)
            .HasConversion(id => id.Value, value => new LeaveRequestId(value))
            .IsRequired();

        builder.Property(log => log.LeaveRequestApprovalAssignmentId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new LeaveRequestApprovalAssignmentId(value.Value) : null);

        builder.Property(log => log.PreviousApproverEmployeeId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new EmployeeId(value.Value) : null);

        builder.Property(log => log.NewApproverEmployeeId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new EmployeeId(value.Value) : null);

        builder.Property(log => log.ActionType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(log => log.OldAssignmentStatus)
            .HasMaxLength(50);

        builder.Property(log => log.NewAssignmentStatus)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(log => log.ReasonCode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(log => log.Note)
            .HasMaxLength(500);

        builder.Property(log => log.CreatedByUserId)
            .IsRequired();

        builder.Property(log => log.CreatedDate)
            .IsRequired();

        builder.HasOne(log => log.LeaveRequest)
            .WithMany()
            .HasForeignKey(log => log.LeaveRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_approval_route_audit_log_leave_request_id");

        builder.HasOne(log => log.LeaveRequestApprovalAssignment)
            .WithMany()
            .HasForeignKey(log => log.LeaveRequestApprovalAssignmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_approval_route_audit_log_assignment_id");

        builder.HasOne(log => log.PreviousApprover)
            .WithMany()
            .HasForeignKey(log => log.PreviousApproverEmployeeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_approval_route_audit_log_employee_previous_approver_id");

        builder.HasOne(log => log.NewApprover)
            .WithMany()
            .HasForeignKey(log => log.NewApproverEmployeeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_approval_route_audit_log_employee_new_approver_id");

        builder.HasIndex(log => new { log.LeaveRequestId, log.CreatedDate });
        builder.HasIndex(log => log.LeaveRequestApprovalAssignmentId);
    }
}
