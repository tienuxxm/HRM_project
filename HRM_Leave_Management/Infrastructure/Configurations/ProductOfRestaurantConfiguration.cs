using Domain.ProductOfRestaurants;
using Domain.Products;
using Domain.Restaurants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal class ProductOfRestaurantConfiguration : IEntityTypeConfiguration<ProductOfRestaurant>
{
    public void Configure(EntityTypeBuilder<ProductOfRestaurant> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ProductRestaurantId(value));


        builder.Property(x => x.ProductId)
            .HasConversion(id => id.Value, value => new ProductId(value));

        builder.Property(x => x.RestaurantId)
            .HasConversion(id => id.Value, value => new RestaurantId(value));

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId);

        builder.HasOne(x => x.Restaurant)
            .WithMany()
            .HasForeignKey(x => x.RestaurantId);
    }
}