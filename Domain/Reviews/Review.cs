using Domain.Abstractions;
using Domain.Bookings;
using Domain.Members;
using Domain.Orders;
using Domain.Restaurants;
using Domain.Reviews.Events;

namespace Domain.Reviews;

public sealed class Review : Entity<ReviewId>
{
    private Review(
        ReviewId id,
        OrderId orderId,
        MemberId memberId,
        Rating rating,
        Comment comment,
        DateTime createdDate)
        : base(id)
    {
        OrderId = orderId;
        MemberId = memberId;
        Rating = rating;
        Comment = comment;
        CreatedDate = createdDate;
    }

    private Review()
    {
    }

    public OrderId OrderId { get; private set; }

    public MemberId MemberId { get; private set; }

    public Rating Rating { get; private set; }

    public Comment Comment { get; private set; }

    public DateTime CreatedDate { get; private set; }

    public static Result<Review> Create(
        Order order,
        Rating rating,
        Comment comment,
        DateTime createdOnUtc)
    {
        if (order.Status != OrderStatus.Done)
        {
            return Result.Failure<Review>(ReviewErrors.NotEligible);
        }

        var review = new Review(
            ReviewId.New(),
            order.Id,
            order.MemberId,
            rating,
            comment,
            createdOnUtc);

        review.RaiseDomainEvent(new ReviewCreatedDomainEvent(review.Id));

        return review;
    }
}