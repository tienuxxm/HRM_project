using Domain.ApprovalRouting;
using Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class ApprovalRouteLevelAssignmentConfiguration : IEntityTypeConfiguration<ApprovalRouteLevelAssignment>
{
    public void Configure(EntityTypeBuilder<ApprovalRouteLevelAssignment> builder)
    {
        builder.ToTable("approval_route_level_assignment");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => new ApprovalRouteLevelAssignmentId(value));

        builder.Property(a => a.ApprovalRouteLevelId)
            .HasConversion(id => id.Value, value => new ApprovalRouteLevelId(value))
            .IsRequired();

        builder.Property(a => a.AssignedEmployeeId)
            .HasConversion(id => id.Value, value => new EmployeeId(value))
            .IsRequired();

        builder.Property(a => a.EffectiveFrom)
            .IsRequired();

        builder.Property(a => a.EffectiveTo);

        builder.Property(a => a.IsActive)
            .IsRequired();

        builder.Property(a => a.Reason)
            .HasMaxLength(500);

        builder.Property(a => a.CreatedByUserId)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.HasOne(a => a.ApprovalRouteLevel)
            .WithMany(l => l.Assignments)
            .HasForeignKey(a => a.ApprovalRouteLevelId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_approval_route_level_assignment_level_level_id");

        builder.HasOne(a => a.AssignedEmployee)
            .WithMany()
            .HasForeignKey(a => a.AssignedEmployeeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_approval_route_level_assignment_employee_assigned_employee_id");

        // Indexes for performance
        builder.HasIndex(a => new { a.ApprovalRouteLevelId, a.IsActive });
        builder.HasIndex(a => new { a.AssignedEmployeeId, a.IsActive });
    }
}
