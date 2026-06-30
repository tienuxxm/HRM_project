using Domain.Orders;
using Domain.Products;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal class LineItemConfiguration : IEntityTypeConfiguration<LineItem>
{
    public void Configure(EntityTypeBuilder<LineItem> builder)
    {
        builder.HasKey(li => li.Id);

        builder.Property(li => li.Id).HasConversion(
            lineItemId => lineItemId.Value,
            value => new LineItemId(value));

        builder.Property(li => li.ProductImageUrl)
            .HasConversion(img => img.Value, value => new ImageUrl(value));

        builder.Property(li => li.ProductName)
            .HasMaxLength(250)
            .HasConversion(name => name.Value, value => new ProductName(value));

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(li => li.ProductId);

        builder.Property(x => x.Note)
            .HasConversion(note => note.Value, value => new Note(value));

        builder.OwnsOne(li => li.Price, priceBuilder =>
        {
            priceBuilder.Property(money => money.Amount)
                .IsRequired();
            priceBuilder.Property(money => money.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });
    }
}