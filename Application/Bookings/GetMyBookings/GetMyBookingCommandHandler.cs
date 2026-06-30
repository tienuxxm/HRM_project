using Application.Abstractions.Authentication;
using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Bookings.GetBooking;
using Application.Orders.GetOrder;
using Domain.Abstractions;
using Domain.Bookings;
using Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace Application.Bookings.GetMyBookings;

public class GetMyBookingCommandHandler : ICommandHandler<GetMyBookingsCommand, List<BookingResponse>>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IAwsS3Service _awsS3Service;
    private readonly IMemberContext _memberContext;
    private readonly IBookingRepository _bookingRepository;

    public GetMyBookingCommandHandler(IAwsS3Service awsS3Service, IMemberRepository memberRepository,
        IMemberContext memberContext, IBookingRepository bookingRepository)
    {
        _awsS3Service = awsS3Service;
        _memberRepository = memberRepository;
        _memberContext = memberContext;
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<List<BookingResponse>>> Handle(GetMyBookingsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(_memberContext.IdentityId))
                return Result.Failure<List<BookingResponse>>(MemberErrors.NotFound);
            var member = await _memberRepository.GetByIdentityAsync(_memberContext.IdentityId, cancellationToken);
            if (member is null)
                return Result.Failure<List<BookingResponse>>(MemberErrors.NotFound);
            var result = await _bookingRepository.GetEntitiesAsQueryable()
                .AsNoTracking()
                .Include(x => x.Restaurant)
                .Include(x => x.Order)
                .ThenInclude(x => x.LineItems)
                .Where(x => x.MemberId.Equals(member.Id)).ToListAsync(cancellationToken);
            var bookingDtos = result
                .Select(booking => new BookingResponse()
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
                    LineItemResponses = booking?.Order?.LineItems?.Select(l => new LineItemResponse()
                    {
                        Id = l.Id.Value,
                        Price = l.Price,
                        Quantity = l.Quantity,
                        ProductName = l.ProductName.Value,
                        ProductImage = _awsS3Service.GetUrlPresign(l?.ProductImageUrl?.Value ?? "", 60)
                    }).ToList()
                }).ToList();
            return Result.Success(bookingDtos);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}