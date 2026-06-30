using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Members;
using Domain.Shared;

namespace Application.Members.ChangeAvatar;

public class MemberChangeAvatarComandHandler : ICommandHandler<MemberChangeAvatarCommand>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemberContext _memberContext;

    public MemberChangeAvatarComandHandler(IMemberRepository memberRepository, IUnitOfWork unitOfWork,
        IMemberContext memberContext)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _memberContext = memberContext;
    }

    public async Task<Result> Handle(MemberChangeAvatarCommand request, CancellationToken cancellationToken)
    {
        var memberIdentity = _memberContext.IdentityId;
        if (string.IsNullOrEmpty(memberIdentity))
            return Result.Failure(MemberErrors.InvalidCredentials);
        var member = await _memberRepository.GetByIdentityAsync(memberIdentity, cancellationToken);
        if (member is null)
            return Result.Failure(MemberErrors.NotFound);
        member.SetAvatar(new ImageUrl(request.Image));
        _memberRepository.Update(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}