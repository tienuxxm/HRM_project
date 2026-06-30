using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Members;

namespace Application.Members.DeleteMyAccount;

public class DeleteMyAccountCommandHandler : ICommandHandler<DeleteMyAccountCommand, bool>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IMemberContext _memberContext;
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMyAccountCommandHandler(IMemberContext memberContext, IMemberRepository memberRepository,
        IUnitOfWork unitOfWork, IAuthenticationService authenticationService)
    {
        _memberContext = memberContext;
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _authenticationService = authenticationService;
    }

    public async Task<Result<bool>> Handle(DeleteMyAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _memberContext.IdentityId;
        var member = await _memberRepository.GetByIdentityAsync(userId, cancellationToken);
        if (member is null)
            return Result.Failure<bool>(MemberErrors.NotFound);
        var result = await _authenticationService.DeleteUser(userId, cancellationToken);
        if (!result.IsSuccess)
            return result.IsSuccess ? Result.Success(true) : Result.Failure<bool>(MemberErrors.NotFound);
        member.Deactivate();
        _memberRepository.Update(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result.IsSuccess ? Result.Success(true) : Result.Failure<bool>(MemberErrors.NotFound);
    }
}