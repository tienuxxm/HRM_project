using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.PromotionToRestaurants;

namespace Infrastructure.Configurations
{
    internal sealed class PromotionToRestaurantConfiguration : IEntityTypeConfiguration<PromotionToRestaurant>
    {
        public void Configure(EntityTypeBuilder<PromotionToRestaurant> builder)
        {
            builder.ToTable("promotion-to-restaurant");

            builder.HasKey(pr => new { pr.PromotionId, pr.RestaurantId, pr.Id });

            builder.Property(p => p.Id)
                .HasConversion(pId => pId.Value, value => new PromotionToRestaurantId(value));

            builder.HasOne(x => x.Promotion)
                .WithMany()
                .HasForeignKey(x => x.PromotionId);
            builder.HasOne(x => x.Restaurant)
                .WithMany()
                .HasForeignKey(x => x.RestaurantId);
        }
    }
}