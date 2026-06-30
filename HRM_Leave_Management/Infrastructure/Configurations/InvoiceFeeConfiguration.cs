using Domain.InvoiceFees;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class InvoiceFeeConfiguration : IEntityTypeConfiguration<InvoiceFee>
{
    public void Configure(EntityTypeBuilder<InvoiceFee> builder)
    {
        builder.ToTable("invoice_fee");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new InvoiceFeeId(value));

        builder.Property(x => x.InvoiceFeeName)
            .HasConversion(name => name.Value, value => new InvoiceFeeName(value));

        builder.Property(x => x.InvoiceFeeAmount)
            .HasConversion(amount => amount.Value, value => new InvoiceFeeAmount(value));

        builder.OwnsOne(li => li.FeeChange, priceBuilder =>
        {
            priceBuilder.Property(money => money.Amount)
                .IsRequired();
            priceBuilder.Property(money => money.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });
    }
}