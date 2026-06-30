using Application.Abstractions.Authentication;
using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Bookings.GetBooking;
using Application.Orders.GetOrder;
using Domain.Abstractions;
using Domain.Bookings;
using Domain.Members;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Application.Bookings.GetMyBookingPaged;

public class GetMyBookingPagedCommandHandler : ICommandHandler<GetMyBookingPagedCommand, PagedList<BookingResponse>>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IAwsS3Service _awsS3Service;
    private readonly IMemberContext _memberContext;
    private readonly IBookingRepository _bookingRepository;

    public GetMyBookingPagedCommandHandler(IMemberRepository memberRepository, IAwsS3Service awsS3Service,
        IMemberContext memberContext, IBookingRepository bookingRepository)
    {
        _memberRepository = memberRepository;
        _awsS3Service = awsS3Service;
        _memberContext = memberContext;
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<PagedList<BookingResponse>>> Handle(GetMyBookingPagedCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(_memberContext.IdentityId))
                return Result.Failure<PagedList<BookingResponse>>(MemberErrors.NotFound);
            var member = await _memberRepository.GetByIdentityAsync(_memberContext.IdentityId, cancellationToken);
            if (member is null)
                return Result.Failure<PagedList<BookingResponse>>(MemberErrors.NotFound);
            var query = _bookingRepository.GetEntitiesAsQueryable()
                .OrderByDescending(x => x.CreateDate)
                .AsNoTracking()
                .Include(x => x.Restaurant)
                .Include(x => x.Order)
                .ThenInclude(x => x.LineItems)
                .Where(x => x.MemberId.Equals(member.Id));
            var result = await _bookingRepository.GetAllPaged(request, query);
            var bookingDtos = result.Data
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
                        ProductImage = _awsS3Service.GetUrlPresign(l.ProductImageUrl!.Value, 60)
                    }).ToList()
                }).ToList();

            return Result.Success(new PagedList<BookingResponse>(bookingDtos, result.TotalCount,
                result.CurrentPage, result.PageSize));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}