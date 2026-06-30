using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MemberDeviceTokens;
using Domain.Members;

namespace Application.Members.LogInMember;

internal sealed class LogInMemberCommandHandler : ICommandHandler<LogInMemberCommand, AccessTokenResponse>
{
    private readonly IMemberDeviceTokenRepository _deviceTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogInMemberCommandHandler(IJwtService jwtService, IMemberRepository memberRepository,
        IMemberDeviceTokenRepository deviceTokenRepository, IUnitOfWork unitOfWork)
    {
        _jwtService = jwtService;
        _memberRepository = memberRepository;
        _deviceTokenRepository = deviceTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AccessTokenResponse>> Handle(
        LogInMemberCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var member = await _memberRepository.GetByPhoneNumberAsync(request.Phone, cancellationToken);
            if (member is null)
                return Result.Failure<AccessTokenResponse>(MemberErrors.NotFound);
            if (!member.IsActive)
                return Result.Failure<AccessTokenResponse>(MemberErrors.NotFound);
            var result = await _jwtService.GetAccessTokenAsync(
                member.Email.Value,
                request.Password,
                cancellationToken);

            if (result.IsFailure)
                return Result.Failure<AccessTokenResponse>(MemberErrors.InvalidCredentials);
            if (string.IsNullOrEmpty(request.deviceToken)) return Result.Success(new AccessTokenResponse(result.Value));
            var deviceToken = MemberDeviceToken.Create(member.IdentityId, request.deviceToken);
            _deviceTokenRepository.Add(deviceToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new AccessTokenResponse(result.Value));
        }
        catch (Exception e)
        {
            return Result.Failure<AccessTokenResponse>(new Error("Login.Fail",
                "Có lỗi xảy ra trong quá trình đăng nhập"));
        }
    }
}