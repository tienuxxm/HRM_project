using Domain.ApprovalRouting;
using Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class ApprovalRoutePolicyConfiguration : IEntityTypeConfiguration<ApprovalRoutePolicy>
{
    public void Configure(EntityTypeBuilder<ApprovalRoutePolicy> builder)
    {
        builder.ToTable("approval_route_policy");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => new ApprovalRoutePolicyId(value));

        builder.Property(p => p.DepartmentId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new DepartmentId(value.Value) : null)
            .IsRequired(false);

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.HasOne(p => p.Department)
            .WithMany()
            .HasForeignKey(p => p.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_approval_route_policy_department_department_id");

        builder.HasMany(p => p.Levels)
            .WithOne(l => l.Policy)
            .HasForeignKey(l => l.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Rules)
            .WithOne(r => r.Policy)
            .HasForeignKey(r => r.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Filtered unique index for active department policy: At most ONE active policy per department
        builder.HasIndex(p => p.DepartmentId)
            .IsUnique()
            .HasFilter("is_active = true AND department_id IS NOT NULL")
            .HasDatabaseName("ix_approval_route_policy_department_id_active_dept");

        // Note: Active company-level policy (department_id IS NULL) expression index ON approval_route_policy ((1)) WHERE is_active = true AND department_id IS NULL
        // is added via raw SQL migration to ensure 100% PostgreSQL uniqueness compliance.
    }
}
