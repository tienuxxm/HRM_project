using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Orders.GetOrder;
using Domain.Abstractions;
using Domain.Bookings;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Application.Bookings.GetBooking;

public class GetBookingCommandHandler : ICommandHandler<GetBookingCommand, BookingResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IAwsS3Service _awsS3Service;
    private readonly IOrderRepository _orderRepository;

    public GetBookingCommandHandler(IBookingRepository bookingRepository, IAwsS3Service awsS3Service,
        IOrderRepository orderRepository)
    {
        _bookingRepository = bookingRepository;
        _awsS3Service = awsS3Service;
        _orderRepository = orderRepository;
    }

    public async Task<Result<BookingResponse>> Handle(GetBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetEntitiesAsQueryable().Include(x => x.Member)
            .Include(x => x.Restaurant)
            .Include(x => x.Order)
            .ThenInclude(x => x.LineItems)
            .FirstOrDefaultAsync(x => x.Id.Equals(new BookingId(request.BookingId)), cancellationToken);
        if (booking is null)
            return Result.Failure<BookingResponse>(BookingErrors.NotFound);

        var bookingResponse = new BookingResponse()
        {
            Id = booking.Id.Value,
            Status = booking.Status,
            BookingCode = booking.BookingCode.Value,
            BookingTime = booking.BookingTime,
            CreatedDate = booking.CreateDate,
            MemberName = booking.FullName.Value,
            RestaurantId = booking.RestaurantId.Value,
            TotalOfPeople = booking.TotalOfPeople,
            RestaurantName = booking.Restaurant.RestaurantName.Value,
            PhoneNumber = booking.PhoneNumber.Value,
            Note = booking.Note?.Value,
            RestaurantAddress = booking.Restaurant.Address.Street + " " + booking.Restaurant.Address.State + " " + booking.Restaurant.Address.City,
            LineItemResponses = booking.Order?.LineItems.Select(l => new LineItemResponse()
            {
                Id = l.Id.Value,
                Price = l.Price,
                Quantity = l.Quantity,
                ProductName = l.ProductName.Value,
                Note = l.Note?.Value,
                ProductImage = _awsS3Service.GetUrlPresign(l.ProductImageUrl!.Value, 60)
            }).ToList()
        };
        return Result.Success(bookingResponse);
    }
}