using Domain.Categories;
using Domain.Orders;
using Domain.Products;
using Domain.Restaurants;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ImageUrl = Domain.Shared.ImageUrl;

namespace Infrastructure.Configurations;

internal class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasConversion(
            productId => productId.Value,
            value => new ProductId(value));

        builder.Property(p => p.CategoryId).IsRequired().HasConversion(
            catId => catId.Value,
            value => new CategoryId(value));


        builder.Property(p => p.ProductName)
            .HasMaxLength(250)
            .HasConversion(name => name.Value, value => new ProductName(value));

        builder.Property(x => x.ImageUrl)
            .HasConversion(image => image.Value, value => new ImageUrl(value));


        builder.Property(p => p.Sku).HasConversion(
            sku => sku.Value,
            value => Sku.Create(value));

        builder.OwnsOne(li => li.Price, priceBuilder =>
        {
            priceBuilder.Property(money => money.Amount)
                .IsRequired();
            priceBuilder.Property(money => money.Currency)
                .IsRequired()
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(booking => booking.CategoryId);
    }
}