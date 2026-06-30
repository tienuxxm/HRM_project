using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Application.Orders.Create;
using Domain.Abstractions;
using Domain.Bookings;
using Domain.Members;
using Domain.Orders;
using Domain.Restaurants;
using Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Address = Domain.Members.Address;
using Email = Domain.Members.Email;
using Note = Domain.Bookings.Note;
using PhoneNumber = Domain.Shared.PhoneNumber;

namespace Application.Bookings.UpdateBooking;

public class UpdateBookingCommandHandler : ICommandHandler<UpdateBookingCommand>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly ISender _sender;

    public UpdateBookingCommandHandler(IMemberRepository memberRepository, IBookingRepository bookingRepository,
        IDateTimeProvider dateTimeProvider, IRestaurantRepository restaurantRepository, IUnitOfWork unitOfWork,
        ISender sender, IOrderRepository orderRepository)
    {
        _memberRepository = memberRepository;
        _bookingRepository = bookingRepository;
        _dateTimeProvider = dateTimeProvider;
        _restaurantRepository = restaurantRepository;
        _unitOfWork = unitOfWork;
        _sender = sender;
        _orderRepository = orderRepository;
    }

    public async Task<Result> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var member = await _memberRepository.GetByPhoneNumberAsync(request.PhoneNumeber, cancellationToken);
            if (member is null)
            {
                var latestMember = await _memberRepository.GetLatestByProperty(x => x.MemberCode, cancellationToken);
                var latestBookingCode = latestMember != null ? latestMember.MemberCode.Value : string.Empty;
                var code = string.IsNullOrEmpty(latestBookingCode)
                    ? "0".PadLeft(5, '0')
                    : latestBookingCode.Remove(0, 2);
                var newCode = "KH" + (int.Parse(code) + 1).ToString().PadLeft(5, '0');
                member = Member.Create(
                    new Code(newCode),
                    new FirstName(request.FullName),
                    new LastName(""),
                    new Email(""),
                    new Domain.Members.PhoneNumber(request.PhoneNumeber),
                    new Address(""),
                    _dateTimeProvider.UtcNow,
                    _dateTimeProvider.UtcNow.AddYears(-18),
                    null
                );
                _memberRepository.Add(member);
            }

            var restaurant =
                await _restaurantRepository.GetByIdAsync(new RestaurantId(request.RestaurantId), cancellationToken);

            if (restaurant is null)
            {
                return Result.Failure<Booking>(RestaurantErrors.NotFound);
            }

            var booking = await _bookingRepository.GetByIdAsync(new BookingId(request.BookingId), cancellationToken);
            if (booking is null)
                return Result.Failure(BookingErrors.NotFound);

            var fullname = request.FullName;
            var phoneNumber = request.PhoneNumeber;
            var note = string.IsNullOrEmpty(request.Note) ? null : new Note(request.Note);

            booking.Update(
                member.Id,
                restaurant.Id,
                request.BookingDate,
                request.TotalOfPeople,
                new PhoneNumber(phoneNumber),
                new FullName(fullname),
                note);

            _bookingRepository.Update(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (request.LineItems is not null)
            {
                var lastOrder = await _orderRepository.GetEntitiesAsQueryable()
                    .FirstOrDefaultAsync(x => x.BookingId != null && x.BookingId.Equals(booking.Id), cancellationToken);
                if (lastOrder is not null)
                    _orderRepository.Remove(lastOrder);
                var orderCommand =
                    new CreateOrUpdateOrderCommand(member.Id.Value, null, request.LineItems, null, null, booking,
                        OrderType.Booking, lastOrder?.Id.Value);
                var orderResult = await _sender.Send(orderCommand, cancellationToken);
                if (orderResult.IsFailure)
                {
                    return Result.Failure(BookingErrors.Overlap);
                }
            }

            return Result.Success();
        }
        catch (Exception exception)
        {
            return Result.Failure(Error.NullValue);
        }
    }
}