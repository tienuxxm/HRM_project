using Domain.Products;
using Domain.RestaurantMenuProducts;
using Domain.RestaurantMenus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal class RestaurantMenuProductConfiguration : IEntityTypeConfiguration<RestaurantMenuProduct>
{
    public void Configure(EntityTypeBuilder<RestaurantMenuProduct> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new RestaurantMenuProductId(value));

        builder.Property(x => x.ProductId)
            .HasConversion(id => id.Value, value => new ProductId(value));

        builder.Property(x => x.RestaurantMenuId)
            .HasConversion(id => id.Value, value => new RestaurantMenuId(value));

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId);
    }
}