using Domain.ApprovalRouting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class ApprovalRouteRuleCandidateConfiguration : IEntityTypeConfiguration<ApprovalRouteRuleCandidate>
{
    public void Configure(EntityTypeBuilder<ApprovalRouteRuleCandidate> builder)
    {
        builder.ToTable("approval_route_rule_candidate");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new ApprovalRouteRuleCandidateId(value));

        builder.Property(c => c.ApprovalRouteRuleId)
            .HasConversion(id => id.Value, value => new ApprovalRouteRuleId(value))
            .IsRequired();

        builder.Property(c => c.ApprovalRouteLevelId)
            .HasConversion(id => id.Value, value => new ApprovalRouteLevelId(value))
            .IsRequired();

        builder.Property(c => c.PriorityOrder)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.HasOne(c => c.ApprovalRouteRule)
            .WithMany(r => r.Candidates)
            .HasForeignKey(c => c.ApprovalRouteRuleId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_approval_route_rule_candidate_rule_rule_id");

        builder.HasOne(c => c.ApprovalRouteLevel)
            .WithMany()
            .HasForeignKey(c => c.ApprovalRouteLevelId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_approval_route_rule_candidate_level_level_id");

        // Unique active PriorityOrder per Rule
        builder.HasIndex(c => new { c.ApprovalRouteRuleId, c.PriorityOrder })
            .IsUnique()
            .HasFilter("is_active = true");

        // Unique active ApprovalRouteLevelId per Rule
        builder.HasIndex(c => new { c.ApprovalRouteRuleId, c.ApprovalRouteLevelId })
            .IsUnique()
            .HasFilter("is_active = true");
    }
}
