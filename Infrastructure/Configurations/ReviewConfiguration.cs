using Domain.Bookings;
using Domain.Members;
using Domain.Orders;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(review => review.Id);

        builder.Property(review => review.Id)
            .HasConversion(reviewId => reviewId.Value, value => new ReviewId(value));

        builder.Property(review => review.Rating)
            .HasConversion(rating => rating.Value, value => Rating.Create(value).Value);

        builder.Property(review => review.Comment)
            .HasMaxLength(2000)
            .HasConversion(comment => comment.Value, value => new Comment(value));

        builder.HasOne<Order>()
    .WithMany()
    .HasForeignKey(review => review.OrderId);
     

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(review => review.MemberId);
    }
}