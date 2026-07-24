using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class ApprovalRouteRuleConfiguration : IEntityTypeConfiguration<ApprovalRouteRule>
{
    public void Configure(EntityTypeBuilder<ApprovalRouteRule> builder)
    {
        builder.ToTable("approval_route_rule", t =>
        {
            t.HasCheckConstraint(
                "ck_approval_route_rule_auto_approve_no_specific_approver",
                "is_auto_approve = false OR specific_approver_employee_id IS NULL");
        });

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => new ApprovalRouteRuleId(value));

        builder.Property(r => r.PolicyId)
            .HasConversion(id => id.Value, value => new ApprovalRoutePolicyId(value))
            .IsRequired();

        builder.Property(r => r.RequesterPositionId)
            .HasConversion(id => id.Value, value => new PositionId(value))
            .IsRequired();

        builder.Property(r => r.SpecificApproverEmployeeId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new EmployeeId(value.Value) : null);

        builder.Property(r => r.IsAutoApprove)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(r => r.IsActive)
            .IsRequired();

        builder.HasOne(r => r.Policy)
            .WithMany(p => p.Rules)
            .HasForeignKey(r => r.PolicyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_approval_route_rule_approval_route_policy_policy_id");

        builder.HasOne(r => r.RequesterPosition)
            .WithMany()
            .HasForeignKey(r => r.RequesterPositionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_approval_route_rule_position_requester_position_id");

        builder.HasOne(r => r.SpecificApprover)
            .WithMany()
            .HasForeignKey(r => r.SpecificApproverEmployeeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_approval_route_rule_employee_specific_approver_id");

        builder.HasMany(r => r.Candidates)
            .WithOne(c => c.ApprovalRouteRule)
            .HasForeignKey(c => c.ApprovalRouteRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique active rule per Policy + RequesterPositionId
        builder.HasIndex(r => new { r.PolicyId, r.RequesterPositionId })
            .IsUnique()
            .HasFilter("is_active = true");
    }
}
