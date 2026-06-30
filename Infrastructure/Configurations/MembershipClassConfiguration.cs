using Domain.MembershipClasses;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class MembershipClassConfiguration : IEntityTypeConfiguration<MembershipClass>
{
    public void Configure(EntityTypeBuilder<MembershipClass> builder)
    {
        builder.ToTable("membership_class");

        builder.HasKey(membership => membership.Id);

        builder.Property(membership => membership.Id)
            .HasConversion(id => id.Value, value => new MembershipClassId(value));
        
        builder.Property(membership => membership.Level)
            .HasConversion(level => level.Value, value => new Level(value));

        builder.Property(membership => membership.ClassName)
            .HasConversion(className => className.Value, value => new ClassName(value));

        builder.OwnsOne(li => li.MaxMoney, priceBuilder =>
        {
            priceBuilder.Property(money => money.Amount)
                .IsRequired();
            priceBuilder.Property(money => money.Currency)
                .IsRequired()
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });

        builder.HasMany(membership => membership.MembershipBenefits)
            .WithOne()
            .HasForeignKey(benefit => benefit.MembershipClassId);
    }
}