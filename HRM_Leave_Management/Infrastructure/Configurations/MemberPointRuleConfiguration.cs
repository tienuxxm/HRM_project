using Domain.MemberPointRules;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class MemberPointRuleConfiguration : IEntityTypeConfiguration<MemberPointRule>
{
    public void Configure(EntityTypeBuilder<MemberPointRule> builder)
    {
        builder.ToTable("member_point_rule");

        builder.HasKey(point => point.Id);

        builder.Property(point => point.Id)
            .HasConversion(id => id.Value, value => new MemberPointRuleId(value));

        builder.Property(point => point.PointPerAmount)
            .HasConversion(pa => pa.Value, value => new PointPerAmount(value));

        builder.OwnsOne(point => point.MinimumAmount, priceBuilder =>
        {
            priceBuilder.Property(money => money.Amount)
                .IsRequired();
            priceBuilder.Property(money => money.Currency)
                .IsRequired()
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });
    }
}