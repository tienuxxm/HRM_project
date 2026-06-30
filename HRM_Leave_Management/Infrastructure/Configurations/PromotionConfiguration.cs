using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Promotions;
using Domain.Shared;

namespace Infrastructure.Configurations
{
    internal sealed class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
    {
        public void Configure(EntityTypeBuilder<Promotion> builder)
        {
            builder.ToTable("promotion");

            builder.HasKey(promotion => promotion.Id);

            builder.Property(promotion => promotion.Id)
                .HasConversion(promotionId => promotionId.Value, value => new PromotionId(value));

            builder.Property(p => p.Content)
                .IsRequired(true)
                .HasConversion(content => content.Value, value => new PromotionContent(value));

            builder.Property(p => p.PromotionName)
                .IsRequired(true)
                .HasConversion(content => content.Value, value => new PromotionName(value));

            builder.Property(p => p.ImageUrl)
                .IsRequired(false)
                .HasConversion(image => image.Value, value => new ImageUrl(value));

            builder.Property(p => p.Title)
                .IsRequired(true)
                .HasConversion(title => title.Value, value => new PromotionTitle(value));

            builder.Property(p => p.Title)
                .IsRequired(true)
                .HasConversion(title => title.Value, value => new PromotionTitle(value));

            builder.Property(pro => pro.CreatedDate)
                .IsRequired();

            builder.Property(pro => pro.StartedAt)
                .IsRequired();

            builder.Property(pro => pro.EndedAt)
                .IsRequired();
        }
    }
}