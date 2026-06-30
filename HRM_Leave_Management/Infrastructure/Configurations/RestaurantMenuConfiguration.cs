using Domain.RestaurantMenus;
using Domain.Restaurants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal class RestaurantMenuConfiguration : IEntityTypeConfiguration<RestaurantMenu>
{
    public void Configure(EntityTypeBuilder<RestaurantMenu> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new RestaurantMenuId(value));

        builder.Property(x => x.RestaurantId)
            .HasConversion(x => x.Value, value => new RestaurantId(value));

        builder.HasMany(x => x.MenuProducts)
            .WithOne(x => x.RestaurantMenu)
            .HasForeignKey(x => x.RestaurantMenuId);
    }
}