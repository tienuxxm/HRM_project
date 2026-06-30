using Application.Abstractions.Authentication;
using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Districts;
using Domain.MemberNotifications;
using Domain.Members;
using Domain.MembershipClasses;
using Domain.MemberVouchers;
using Domain.Notifications;
using Domain.QrCode;
using Domain.Shared;
using Domain.Vouchers;
using Microsoft.EntityFrameworkCore;
using Content = Domain.MemberNotifications.Content;
using Email = Domain.Members.Email;
using PhoneNumber = Domain.Members.PhoneNumber;
using ReferenceId = Domain.MemberNotifications.ReferenceId;

namespace Application.Members.RegisterMember;

internal sealed class RegisterMemberCommandHandler : ICommandHandler<RegisterMemberCommand, Guid>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMemberRepository _memberRepository;
    private readonly IMembershipClassRepository _membershipClassRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVoucherRepository _voucherRepository;
    private readonly IQrCodeRepository _qrCodeRepository;
    private readonly IMemberNotificationRepository _memberNotificationRepository;
    private readonly IMemberVoucherRepository _memberVoucherRepository;

    public RegisterMemberCommandHandler(
        IAuthenticationService authenticationService,
        IMemberRepository memberRepository,
        IUnitOfWork unitOfWork, IMembershipClassRepository membershipClassRepository,
        IDateTimeProvider dateTimeProvider, IVoucherRepository voucherRepository, IQrCodeRepository qrCodeRepository,
        IMemberNotificationRepository memberNotificationRepository, IMemberVoucherRepository memberVoucherRepository)
    {
        _authenticationService = authenticationService;
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _membershipClassRepository = membershipClassRepository;
        _dateTimeProvider = dateTimeProvider;
        _voucherRepository = voucherRepository;
        _qrCodeRepository = qrCodeRepository;
        _memberNotificationRepository = memberNotificationRepository;
        _memberVoucherRepository = memberVoucherRepository;
    }

    public async Task<Result<Guid>> Handle(
        RegisterMemberCommand request,
        CancellationToken cancellationToken)
    {
        using var transaction = _unitOfWork.BeginTransaction();
        try
        {
            var existedMember =
                await _memberRepository.GetEntitiesAsQueryable().FirstOrDefaultAsync(
                    x => x.IsActive &&
                         x.PhoneNumber.Equals(new PhoneNumber(request.PhoneNumber)),
                    cancellationToken);
            var address = request.Address;
            var birthDate = request.BirthDate;
            if (existedMember is not null)
            {
                if (!string.IsNullOrEmpty(existedMember.IdentityId))
                {
                    if (existedMember.PhoneNumber.Value == request.PhoneNumber)
                        return Result.Failure<Guid>(MemberErrors.PhoneNumberExisted);
                    if (existedMember.Email.Value == request.Email)
                        return Result.Failure<Guid>(MemberErrors.EmailExisted);
                }

                var memberExistedWithEmail = await _memberRepository.GetEntitiesAsQueryable()
                    .FirstOrDefaultAsync(
                        x => x.Email.Equals(new Email(request.Email)) && x.IsActive &&
                             !string.IsNullOrEmpty(x.IdentityId), cancellationToken);

                if (memberExistedWithEmail is not null) return Result.Failure<Guid>(MemberErrors.EmailExisted);

                if (string.IsNullOrEmpty(address) && !string.IsNullOrEmpty(existedMember.Address.Value))
                    address = existedMember.Address.Value;

                if (!birthDate.HasValue && existedMember.BirthDate is not null) birthDate = existedMember.BirthDate;

                existedMember.Update(new FirstName(request.FirstName), new LastName(request.LastName),
                    new Address(address), new PhoneNumber(request.PhoneNumber), new Email(request.Email), birthDate,
                    request.DistrictId.HasValue ? new DistrictId(request.DistrictId.Value) : null, existedMember.Note
                );

                var identityExistedMember = await _authenticationService.RegisterAsync(
                    existedMember,
                    request.Password,
                    cancellationToken);
                if (identityExistedMember.IsFailure)
                    return Result.Failure<Guid>(identityExistedMember.Error);
                existedMember.SetIdentityId(identityExistedMember.Value);
                _memberRepository.Update(existedMember);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return existedMember.Id.Value;
            }

            var latestMember = await _memberRepository.GetLatestByProperty(x => x.MemberCode, cancellationToken);
            var latestBookingCode = latestMember != null ? latestMember.MemberCode.Value : string.Empty;
            var code = string.IsNullOrEmpty(latestBookingCode) ? "0".PadLeft(5, '0') : latestBookingCode.Remove(0, 2);
            var newCode = "KH" + (int.Parse(code) + 1).ToString().PadLeft(5, '0');
            var member = Member.Create(
                new Code(newCode),
                new FirstName(request.FirstName),
                new LastName(request.LastName),
                new Email(request.Email),
                new PhoneNumber(request.PhoneNumber),
                new Address(address),
                _dateTimeProvider.UtcNow,
                birthDate,
                request.DistrictId.HasValue ? new DistrictId(request.DistrictId.Value) : null
            );

            var identityIdResult = await _authenticationService.RegisterAsync(
                member,
                request.Password,
                cancellationToken);
            if (identityIdResult.IsFailure)
                return Result.Failure<Guid>(identityIdResult.Error);
            member.SetIdentityId(identityIdResult.Value);
            var lowestMembershipClass = await _membershipClassRepository.GetLowestMembershipClass(cancellationToken);
            member.AssignMembershipClass(lowestMembershipClass);

            _memberRepository.Add(member);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            transaction.Commit();
            await Task.Delay(new TimeSpan(0, 0, 0, 10), cancellationToken);

            return member.Id.Value;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return Result.Failure<Guid>(new Error("Register.Fail", "Fail to register new member"));
        }
    }
}