using Application.Abstractions.Messaging;
using Application.Bookings.GetBooking;
using Domain.Abstractions;
using Domain.Bookings;
using Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace Application.Bookings.GetAllPaged;

public class GetAllBookingPagedCommandHandler : ICommandHandler<GetAllBookingPagedCommand, PagedList<BookingResponse>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetAllBookingPagedCommandHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<PagedList<BookingResponse>>> Handle(GetAllBookingPagedCommand request,
        CancellationToken cancellationToken)
    {
        var query = _bookingRepository.GetEntitiesAsQueryable()
            .Include(x => x.Member)
            .Include(x => x.Restaurant)
            .OrderByDescending(x => x.CreateDate)
            .AsQueryable();
        if (request.MemberId is not null)
        {
            query = query.Where(x => x.MemberId.Equals(request.MemberId));
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.AsEnumerable().Where(x =>
                    x.FullName.Value.ToLower().Contains(request.SearchTerm.ToLower())
                    || x.PhoneNumber.Value.ToLower().Contains(request.SearchTerm.ToLower())
                    || x.BookingCode.Value.ToLower().Contains(request.SearchTerm.ToLower()))
                .AsQueryable();
        }

        var result = await _bookingRepository.GetAllPaged(request, query);
        var bookingResponse = result.Data.Select(b => new BookingResponse()
        {
            Id = b.Id.Value,
            Status = b.Status,
            BookingCode = b.BookingCode.Value,
            BookingTime = b.BookingTime,
            CreatedDate = b.CreateDate,
            MemberName = b.FullName.Value,
            PhoneNumber = b.PhoneNumber.Value,
            RestaurantId = b.RestaurantId.Value,
            TotalOfPeople = b.TotalOfPeople,
            RestaurantName = b.Restaurant.RestaurantName.Value
        }).ToList();
        return Result.Success(new PagedList<BookingResponse>(bookingResponse,
            result.TotalCount, result.CurrentPage, result.PageSize));
    }
}