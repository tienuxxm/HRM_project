using Domain.Abstractions;
using Domain.Bookings.Events;
using Domain.Restaurants;
using Domain.Members;
using Domain.Orders;
using Domain.Shared;
using PhoneNumber = Domain.Shared.PhoneNumber;

namespace Domain.Bookings;

public sealed class Booking : Entity<BookingId>
{
    private Booking(BookingId id,
        MemberId memberId,
        Code bookingCode,
        RestaurantId restaurantId,
        BookingStatus bookingStatus,
        DateTime bookingTime,
        string totalOfPeople,
        PhoneNumber phoneNumber,
        FullName fullName,
        DateTime createDate,
        Note? note = null) : base(id)
    {
        MemberId = memberId;
        RestaurantId = restaurantId;
        Status = bookingStatus;
        CreateDate = createDate;
        TotalOfPeople = totalOfPeople;
        BookingTime = bookingTime;
        BookingCode = bookingCode;
        PhoneNumber = phoneNumber;
        FullName = fullName;
        Note = note;
    }

    private Booking()
    {
    }

    public Code BookingCode { get; private set; }

    public MemberId MemberId { get; private set; }
    public Member Member { get; private set; } = null;
    public RestaurantId RestaurantId { get; private set; }
    public Restaurant Restaurant { get; private set; }
    public Order? Order { get; private set; } = null;
    public BookingStatus Status { get; private set; }
    public DateTime BookingTime { get; private set; }
    public DateTime CreateDate { get; private set; }
    public string TotalOfPeople { get; private set; }
    public DateTime? ConfirmedTime { get; private set; }

    public DateTime? RejectedTime { get; private set; }

    public DateTime? CompletedTime { get; private set; }

    public DateTime? CancelledTime { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public FullName FullName { get; private set; }
    public Note? Note { get; private set; }

    public static Booking Reserve(
        MemberId memberId,
        Code bookingCode,
        RestaurantId restaurantId,
        DateTime bookingTime,
        string totalOfPeople,
        PhoneNumber phoneNumber,
        FullName fullName,
        DateTime createDate, Note? note = null)
    {
        var booking = new Booking(
            BookingId.New(),
            memberId,
            bookingCode,
            restaurantId,
            BookingStatus.Reserved,
            bookingTime,
            totalOfPeople,
            phoneNumber,
            fullName,
            createDate,
            note
        );
        booking.RaiseDomainEvent(new BookingReservedDomainEvent(booking.Id));
        return booking;
    }

    public void Update(MemberId memberId,
        RestaurantId? restaurantId,
        DateTime? bookingTime,
        string? totalOfPeople,
        PhoneNumber? phoneNumber,
        FullName? fullName,
        Note? note = null)
    {
        MemberId = memberId;
        RestaurantId = restaurantId ?? RestaurantId;
        BookingTime = bookingTime ?? BookingTime;
        TotalOfPeople = totalOfPeople ?? TotalOfPeople;
        PhoneNumber = phoneNumber ?? PhoneNumber;
        FullName = fullName ?? FullName;
        Note = note ?? note;
    }

    public Result Confirm(DateTime utcNow)
    {
        /*if (Status != BookingStatus.Reserved)
        {
            return Result.Failure(BookingErrors.NotReserved);
        }*/

        Status = BookingStatus.Confirmed;
        ConfirmedTime = utcNow;

        RaiseDomainEvent(new BookingConfirmedDomainEvent(Id));

        return Result.Success();
    }

    public Result Reject(DateTime utcNow)
    {
        /*if (Status != BookingStatus.Reserved)
        {
            return Result.Failure(BookingErrors.NotReserved);
        }*/

        Status = BookingStatus.Rejected;
        RejectedTime = utcNow;

        RaiseDomainEvent(new BookingRejectedDomainEvent(Id));

        return Result.Success();
    }

    public Result Complete(DateTime utcNow)
    {
        /*if (Status != BookingStatus.Confirmed)
        {
            return Result.Failure(BookingErrors.NotConfirmed);
        }*/

        Status = BookingStatus.Completed;
        CompletedTime = utcNow;

        RaiseDomainEvent(new BookingCompletedDomainEvent(Id));

        return Result.Success();
    }

    public Result Cancel(DateTime utcNow)
    {
        /*if (Status != BookingStatus.Confirmed)
        {
            return Result.Failure(BookingErrors.NotConfirmed);
        }*/

        Status = BookingStatus.Cancelled;
        CancelledTime = utcNow;
        RaiseDomainEvent(new BookingCancelledDomainEvent(Id));
        return Result.Success();
    }
}