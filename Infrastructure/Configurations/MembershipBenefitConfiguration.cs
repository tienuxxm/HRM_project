using Domain.MembershipBenefits;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class MembershipBenefitConfiguration : IEntityTypeConfiguration<MembershipBenefit>
{
    public void Configure(EntityTypeBuilder<MembershipBenefit> builder)
    {
        builder.ToTable("member_ship_benefit");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new MembershipBenefitId(value));

        builder.Property(x => x.Title)
            .HasConversion(title => title.Value, value => new Title(value));

        builder.Property(x => x.Description)
            .HasConversion(des => des.Value, value => new Description(value));
    }
}