using Domain.InvoiceDetails;
using Domain.Products;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class InvoiceDetailConfiguartion : IEntityTypeConfiguration<InvoiceDetail>
{
    public void Configure(EntityTypeBuilder<InvoiceDetail> builder)
    {
        builder.ToTable("invoice_detail");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasConversion(id => id.Value, value => new InvoiceDetailId(value));

        builder.Property(i => i.ProductName)
            .HasConversion(name => name.Value, value => new ProductName(value));

        builder.OwnsOne(i => i.Price, priceBuilder =>
        {
            priceBuilder.Property(money => money.Amount)
                .IsRequired();
            priceBuilder.Property(money => money.Currency)
                .IsRequired()
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });
    }
}