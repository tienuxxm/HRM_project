using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Bookings;
using Domain.MemberPointHistories;
using Domain.MemberPointRules;
using Domain.Members;
using Domain.MembershipClasses;
using Domain.Shared;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Bookings.CompleteBooking;

internal sealed class CompleteBookingCommandHandler : ICommandHandler<CompleteBookingCommand>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteBookingCommandHandler(
        IDateTimeProvider dateTimeProvider,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork)
    {

        _dateTimeProvider = dateTimeProvider;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CompleteBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(new BookingId(request.BookingId), cancellationToken);

        if (booking is null)
        {
            return Result.Failure(BookingErrors.NotFound);
        }

        var result = booking.Complete(_dateTimeProvider.UtcNow);
     
        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}