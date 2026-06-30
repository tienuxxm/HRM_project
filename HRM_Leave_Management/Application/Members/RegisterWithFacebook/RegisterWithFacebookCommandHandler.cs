using System.IdentityModel.Tokens.Jwt;
using Application.Abstractions.Authentication;
using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Districts;
using Domain.Members;
using Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Email = Domain.Members.Email;
using PhoneNumber = Domain.Members.PhoneNumber;

namespace Application.Members.RegisterWithFacebook;

public class RegisterWithFacebookCommandHandler : ICommandHandler<RegisterWithFacebookCommand, TokenResponse>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IJwtService _jwtService;
    private readonly IMemberRepository _memberRepository;
    private readonly ISender _sender;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterWithFacebookCommandHandler(ISender sender, IMemberRepository memberRepository,
        IJwtService jwtService, IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork)
    {
        _sender = sender;
        _memberRepository = memberRepository;
        _jwtService = jwtService;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TokenResponse>> Handle(RegisterWithFacebookCommand request,
        CancellationToken cancellationToken)
    {
        /*var checkMemberExistedCommand = new IsFacebookMemberExistedCommand(request.IdentityId);
        var checkMemberExistedResult = await _sender.Send(checkMemberExistedCommand, cancellationToken);
        if (checkMemberExistedResult.IsFailure)
            return checkMemberExistedResult;
        if (checkMemberExistedResult.Value)
            return Result.Failure(new Error("REGISTER.FAIL",
                "Account đã tồn tại trên hệ thống, vui lòng đăng nhập"));*/
        try
        {
            var keycloakAccessToken =
                await _jwtService.ExchangeTokenAsync(request.AccessToken, "facebook", cancellationToken);
            if (keycloakAccessToken.IsFailure)
                return keycloakAccessToken;
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadToken(keycloakAccessToken.Value.AccessToken) as JwtSecurityToken;
            var identityId = token != null ? token.Subject : "";
            if (string.IsNullOrEmpty(identityId))
                return Result.Failure<TokenResponse>(new Error("Authentication.Fail", "Đăng kí thất bại"));
            var latestMember = await _memberRepository.GetLatestByProperty(x => x.MemberCode, cancellationToken);
            var latestBookingCode = latestMember != null ? latestMember.MemberCode.Value : string.Empty;
            var code = string.IsNullOrEmpty(latestBookingCode) ? "0".PadLeft(5, '0') : latestBookingCode.Remove(0, 2);
            var newCode = "KH" + (int.Parse(code) + 1).ToString().PadLeft(5, '0');
            var member = Member.Create(new Code(newCode), new FirstName(request.Firstname),
                new LastName(request.Lastname),
                new Email(request.Email), new PhoneNumber(request.PhoneNumber), new Address(request.Address),
                _dateTimeProvider.UtcNow, null,
                request.DistrictId.HasValue ? new DistrictId(request.DistrictId.Value) : null, RegisterType.FACEBOOK);
            member.SetIdentityId(identityId);
            member.SetMemberPlatformIdentityId(request.IdentityId);
            var existedMember = await _memberRepository.GetByIdentityAsync(identityId, cancellationToken);
            if (existedMember is not null)
                return Result.Success(keycloakAccessToken.Value);
            var memberPlatformIdExisted = await _memberRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(
                    x => x.MemberPlatformIdentityId != null && x.MemberPlatformIdentityId.Equals(request.IdentityId) &&
                         x.IdentityId != identityId && x.IsActive,
                    cancellationToken);
            if (memberPlatformIdExisted is not null)
                return Result.Failure<TokenResponse>(RegisterFacebookError.AlreadyRegister);
            var memberEmailExisted = await _memberRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(
                    x => x.IsActive && x.Email.Equals(new Email(request.Email)) && x.IdentityId != identityId,
                    cancellationToken);
            if (memberEmailExisted is not null)
                return Result.Failure<TokenResponse>(RegisterFacebookError.EmailExisted);
            var memberPhoneExisted = await _memberRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(
                    x => x.IsActive && x.PhoneNumber.Equals(new PhoneNumber(request.Email)) &&
                         x.IdentityId != identityId,
                    cancellationToken);
            if (memberPhoneExisted is not null)
                return Result.Failure<TokenResponse>(RegisterFacebookError.PhoneExisted);

            _memberRepository.Add(member);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(keycloakAccessToken.Value);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}