using Domain.RestaurantAreas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class RestaurantAreaConfiguration : IEntityTypeConfiguration<RestaurantArea>
{
    public void Configure(EntityTypeBuilder<RestaurantArea> builder)
    {
        builder.ToTable("restaurant_area");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new RestaurantAreaId(value));

        builder.Property(x => x.AreaName)
            .HasConversion(name => name.Value, value => new AreaName(value));

        builder.HasMany(x => x.Restaurants)
            .WithOne(r => r.RestaurantArea)
            .HasForeignKey(x => x.RestaurantAreaId);
    }
}