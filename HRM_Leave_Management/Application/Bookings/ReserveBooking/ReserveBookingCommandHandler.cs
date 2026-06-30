using Application.Abstractions.Authentication;
using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Application.MemberActivites;
using Domain.Abstractions;
using Domain.Restaurants;
using Domain.Bookings;
using Domain.MemberActivities;
using Domain.Members;
using Domain.Orders;
using Domain.Shared;
using Newtonsoft.Json;
using Address = Domain.Members.Address;
using Email = Domain.Members.Email;
using PhoneNumber = Domain.Shared.PhoneNumber;

namespace Application.Bookings.ReserveBooking;

internal sealed class ReserveBookingCommandHandler : ICommandHandler<ReserveBookingCommand, Booking>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IOrderRepository _orderRepository;
    private readonly IMemberContext _memberContext;

    public ReserveBookingCommandHandler(
        IMemberRepository memberRepository,
        IRestaurantRepository restaurantRepository,
        IBookingRepository bookingRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider, IMemberContext memberContext)
    {
        _memberRepository = memberRepository;
        _restaurantRepository = restaurantRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _memberContext = memberContext;
        _orderRepository = orderRepository;
    }

    public async Task<Result<Booking>> Handle(ReserveBookingCommand request, CancellationToken cancellationToken)
    {
        Member? member;

        if (!string.IsNullOrEmpty(_memberContext.IdentityId) && !request.createdBySystem)
        {
            member = await _memberRepository.GetByIdentityAsync(_memberContext.IdentityId, cancellationToken);
        }
        else
        {
            member = await _memberRepository.GetByPhoneNumberAsync(request.PhoneNumber, cancellationToken);
        }

        //var member = 
        if (member is null)
        {
            var latestMember = await _memberRepository.GetLatestByProperty(x => x.MemberCode, cancellationToken);
            var latestBookingCode = latestMember != null ? latestMember.MemberCode.Value : string.Empty;
            var code = string.IsNullOrEmpty(latestBookingCode) ? "0".PadLeft(5, '0') : latestBookingCode.Remove(0, 2);
            var newCode = "KH" + (int.Parse(code) + 1).ToString().PadLeft(5, '0');
            member = Member.Create(
                new Code(newCode),
                new FirstName(request.FullName),
                new LastName(""),
                new Email(""),
                new Domain.Members.PhoneNumber(request.PhoneNumber),
                new Address(""),
                _dateTimeProvider.UtcNow,
                _dateTimeProvider.UtcNow.AddYears(-18),
                null
            );
            _memberRepository.Add(member);
        }

        var logList = new List<CreateMemberActivity>();
        CreateManyMemberActivityCommand? logCommand;
        logList.Add(new CreateMemberActivity(
            $"Member {member.FullName}, request to reserve booking {JsonConvert.SerializeObject(request)}",
            MemberActivityType.REQUEST));

        var restaurant =
            await _restaurantRepository.GetByIdAsync(new RestaurantId(request.RestaurantId), cancellationToken);

        if (restaurant is null)
        {
            logList.Add(new CreateMemberActivity(
                $"Member {member.FullName}, request to reserve booking {JsonConvert.SerializeObject(request)}, got error: {RestaurantErrors.NotFound.Name}",
                MemberActivityType.REQUEST));
            return Result.Failure<Booking>(RestaurantErrors.NotFound);
        }

        if (!restaurant.IsAvailable || restaurant.IsDeleted)
        {
            return Result.Failure<Booking>(BookingErrors.RestaurantUnavailable);
        }

        try
        {
            var latestBooking = await _bookingRepository.GetLatestByProperty(x => x.BookingCode, cancellationToken);
            var latestBookingCode = latestBooking != null ? latestBooking.BookingCode.Value : string.Empty;
            var code = string.IsNullOrEmpty(latestBookingCode) ? "0".PadLeft(5, '0') : latestBookingCode.Remove(0, 2);
            var newCode = "DB" + (int.Parse(code) + 1).ToString().PadLeft(5, '0');
            var fullname = request.FullName;
            var phoneNumber = request.PhoneNumber;

            var booking = Booking.Reserve(
                member.Id,
                new Code(newCode),
                restaurant.Id,
                request.BookingDate,
                request.TotalOfPeople,
                new PhoneNumber(phoneNumber),
                new FullName(fullname),
                _dateTimeProvider.UtcNow);
            _bookingRepository.Add(booking);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return booking;
        }
        catch (Exception exception)
        {
            return Result.Failure<Booking>(BookingErrors.Overlap);
        }
    }
}