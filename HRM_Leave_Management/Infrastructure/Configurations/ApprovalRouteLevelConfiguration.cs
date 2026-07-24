using Domain.ApprovalRouting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class ApprovalRouteLevelConfiguration : IEntityTypeConfiguration<ApprovalRouteLevel>
{
    public void Configure(EntityTypeBuilder<ApprovalRouteLevel> builder)
    {
        builder.ToTable("approval_route_level");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasConversion(id => id.Value, value => new ApprovalRouteLevelId(value));

        builder.Property(l => l.PolicyId)
            .HasConversion(id => id.Value, value => new ApprovalRoutePolicyId(value))
            .IsRequired();

        builder.Property(l => l.LevelName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(l => l.LevelRank)
            .IsRequired();

        builder.Property(l => l.CanApproveLeave)
            .IsRequired();

        builder.Property(l => l.IsActive)
            .IsRequired();

        builder.HasOne(l => l.Policy)
            .WithMany(p => p.Levels)
            .HasForeignKey(l => l.PolicyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_approval_route_level_approval_route_policy_policy_id");

        builder.HasMany(l => l.Assignments)
            .WithOne(a => a.ApprovalRouteLevel)
            .HasForeignKey(a => a.ApprovalRouteLevelId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique LevelRank active within Policy
        builder.HasIndex(l => new { l.PolicyId, l.LevelRank })
            .IsUnique()
            .HasFilter("is_active = true");
    }
}
