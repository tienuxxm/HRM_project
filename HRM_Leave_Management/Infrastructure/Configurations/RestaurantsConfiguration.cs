using Domain.RestaurantAreas;
using Domain.Restaurants;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class RestaurantsConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("restaurants");

        builder.HasKey(restaurant => restaurant.Id);

        builder.Property(restaurant => restaurant.Id)
            .HasConversion(restaurant => restaurant.Value, value => new RestaurantId(value));

        builder.OwnsOne(restaurant => restaurant.Address);

        builder.Property(restaurant => restaurant.RestaurantName)
            .HasMaxLength(200)
            .HasConversion(name => name.Value, value => new RestaurantName(value));

        builder.Property(x => x.ImageKey)
            .HasConversion(key => key.Value, value => new ImageUrl(value));

        builder.Property(restaurant => restaurant.RestaurantAreaId)
            .HasConversion(id => id.Value, value => new RestaurantAreaId(value));

        builder.HasMany(x => x.Orders)
            .WithOne(x => x.Restaurant)
            .HasForeignKey(x => x.RestaurantId);
    }
}