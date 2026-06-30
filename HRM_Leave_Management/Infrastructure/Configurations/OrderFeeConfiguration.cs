using Application.Abstractions.AWS;
using Domain.OrderFees;
using Domain.Orders;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class OrderFeeConfiguration : IEntityTypeConfiguration<OrderFee>
{
    public void Configure(EntityTypeBuilder<OrderFee> builder)
    {
        builder.ToTable("order_fee");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(id => id.Value, value => new OrderFeeId(value));

        builder.Property(o => o.OrderId)
            .HasConversion(id => id.Value, value => new OrderId(value));

        builder.Property(o => o.OrderFeeName)
            .HasConversion(name => name.Value, value => new OrderFeeName(value));

        builder.Property(o => o.OrderFeeValue)
            .HasConversion(val => val.Value, value => new OrderFeeValue(value));

        builder.OwnsOne(li => li.OrderFeeCharge, priceBuilder =>
        {
            priceBuilder.Property(money => money.Amount)
                .IsRequired();
            priceBuilder.Property(money => money.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });
    }
}