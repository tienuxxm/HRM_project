using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.LeaveRequests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class LeaveRequestApprovalAssignmentConfiguration : IEntityTypeConfiguration<LeaveRequestApprovalAssignment>
{
    public void Configure(EntityTypeBuilder<LeaveRequestApprovalAssignment> builder)
    {
        builder.ToTable("leave_request_approval_assignment");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => new LeaveRequestApprovalAssignmentId(value));

        builder.Property(a => a.LeaveRequestId)
            .HasConversion(id => id.Value, value => new LeaveRequestId(value))
            .IsRequired();

        builder.Property(a => a.AssignedApproverEmployeeId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new EmployeeId(value.Value) : null);

        builder.Property(a => a.AssignmentStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(a => a.AssignmentReason)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(a => a.SnapshotPolicyId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new ApprovalRoutePolicyId(value.Value) : null);

        builder.Property(a => a.SnapshotRuleId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new ApprovalRouteRuleId(value.Value) : null);

        builder.Property(a => a.SnapshotCandidateId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new ApprovalRouteRuleCandidateId(value.Value) : null);

        builder.Property(a => a.SnapshotLevelAssignmentId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new ApprovalRouteLevelAssignmentId(value.Value) : null);

        builder.Property(a => a.AssignedAt)
            .IsRequired();

        builder.HasOne(a => a.LeaveRequest)
            .WithMany()
            .HasForeignKey(a => a.LeaveRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_leave_request_approval_assignment_leave_request_id");

        builder.HasOne(a => a.AssignedApprover)
            .WithMany()
            .HasForeignKey(a => a.AssignedApproverEmployeeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_leave_request_approval_assignment_employee_assigned_approver_id");

        // Unique LeaveRequestId: One current assignment record per LeaveRequest
        builder.HasIndex(a => a.LeaveRequestId)
            .IsUnique();

        // Index for Dashboard W4/W5 fast query
        builder.HasIndex(a => new { a.AssignedApproverEmployeeId, a.AssignmentStatus });
    }
}
