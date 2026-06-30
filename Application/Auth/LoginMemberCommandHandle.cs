using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MemberDeviceTokens;
using Domain.Members;

namespace Application.Auth;

public class LoginMemberCommandHandle : ICommandHandler<LoginMemberCommand, TokenResponse>
{
    private readonly IMemberDeviceTokenRepository _deviceTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoginMemberCommandHandle(IJwtService jwtService, IMemberRepository memberRepository,
        IMemberDeviceTokenRepository deviceTokenRepository, IUnitOfWork unitOfWork)
    {
        _jwtService = jwtService;
        _memberRepository = memberRepository;
        _deviceTokenRepository = deviceTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TokenResponse>> Handle(LoginMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var member = await _memberRepository.GetByPhoneNumberAsync(request.Phone, cancellationToken);
            if (member is null)
                return Result.Failure<TokenResponse>(MemberErrors.NotFound);
            var result = await _jwtService.GetAccessAndRefreshTokenAsync(
                member.Email.Value,
                request.Password,
                cancellationToken);

            if (result.IsFailure)
                return Result.Failure<TokenResponse>(new Error("Login.Fail",
                    "Account or Password incorrect"));
            if (string.IsNullOrEmpty(request.DeviceToken)) return Result.Success(result.Value);
            var isTokenExisted = await _deviceTokenRepository.IsExisted(request.DeviceToken, cancellationToken);
            if (isTokenExisted) return Result.Success(result.Value);

            var deviceToken = MemberDeviceToken.Create(member.IdentityId, request.DeviceToken);
            _deviceTokenRepository.Add(deviceToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(result.Value);
        }
        catch (Exception)
        {
            return Result.Failure<TokenResponse>(new Error("Login.Fail",
                "error occurred during the login process"));
        }
    }
}