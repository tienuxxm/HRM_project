using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace Application.Members.IsFacebookMemberExisted;

public class IsFacebookMemberExistedCommandHandler : ICommandHandler<IsFacebookMemberExistedCommand, bool>
{
    private readonly IMemberRepository _memberRepository;

    public IsFacebookMemberExistedCommandHandler(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<Result<bool>> Handle(IsFacebookMemberExistedCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var member = await _memberRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(
                    x => x.MemberPlatformIdentityId == request.IdentityId && x.RegisterType == RegisterType.FACEBOOK,
                    cancellationToken);
            return Result.Success(member is not null);
        }
        catch (Exception)
        {
            return Result.Failure<bool>(new Error("Member.Facebook.Check", "Có lỗi xảy ra vui lòng thử lại sau"));
        }
    }
}