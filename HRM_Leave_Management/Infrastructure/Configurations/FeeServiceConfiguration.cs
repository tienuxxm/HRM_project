using Domain.FreeServices;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class FeeServiceConfiguration : IEntityTypeConfiguration<FeeService>
{
    public void Configure(EntityTypeBuilder<FeeService> builder)
    {
        builder.ToTable("fee_service");

        builder.HasKey(fs => fs.Id);

        builder.Property(fs => fs.Id)
            .HasConversion(id => id.Value, value => new FeeServiceId(value));

        builder.Property(fs => fs.FeeName)
            .HasConversion(name => name.Value, value => new FeeName(value));

        builder.OwnsOne(li => li.FeeAmount, priceBuilder =>
        {
            priceBuilder.Property(money => money.Amount)
                .IsRequired();
            priceBuilder.Property(money => money.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });
    }
}