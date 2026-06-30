using Domain.Invoices;
using Domain.Orders;
using Domain.PaymentDetails;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoice");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasConversion(id => id.Value, value => new InvoiceId(value));

        builder.Property(i => i.InvoiceCode)
            .HasConversion(code => code.Value, value => new Code(value));

        builder.Property(i => i.OrderId)
            .HasConversion(id => id.Value, value => new OrderId(value));

        builder.Property(i => i.Title)
            .HasConversion(title => title.Value, value => new Title(value));

        builder.OwnsOne(i => i.TotalBill, priceBuilder =>
        {
            priceBuilder.Property(money => money.Amount)
                .IsRequired();
            priceBuilder.Property(money => money.Currency)
                .IsRequired()
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });


        builder.HasOne(x => x.PaymentDetail)
            .WithOne()
            .HasForeignKey<PaymentDetail>(p => p.InvoiceId);

        builder.HasMany(i => i.InvoiceDetails)
            .WithOne()
            .HasForeignKey(i => i.InvoiceId);

        builder.HasMany(i => i.InvoiceFees)
            .WithOne()
            .HasForeignKey(f => f.InvoiceId);
    }
}