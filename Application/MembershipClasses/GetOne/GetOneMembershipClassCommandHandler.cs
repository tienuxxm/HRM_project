using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MembershipClasses;
using Microsoft.EntityFrameworkCore;

namespace Application.MembershipClasses.GetOne;

internal sealed class
    GetOneMembershipClassCommandHandler : ICommandHandler<GetOneMembershipClassCommand, MembershipClassResponse>
{
    private readonly IMembershipClassRepository _membershipClassRepository;

    public GetOneMembershipClassCommandHandler(IMembershipClassRepository membershipClassRepository)
    {
        _membershipClassRepository = membershipClassRepository;
    }

    public async Task<Result<MembershipClassResponse>> Handle(GetOneMembershipClassCommand request,
        CancellationToken cancellationToken)
    {
        var membershipClass = await _membershipClassRepository.GetEntitiesAsQueryable()
            .Include(m => m.MembershipBenefits).FirstOrDefaultAsync(x => x.Id.Equals(request.Id), cancellationToken);
        if (membershipClass is null)
            return Result.Failure<MembershipClassResponse>(MembershipClassErrors.NotFound);
        var membershipClassResponse = new MembershipClassResponse()
        {
            Id = membershipClass.Id.Value,
            Level = membershipClass.Level.Value,
            ClassName = membershipClass.ClassName.Value,
            IsActive = membershipClass.IsActive,
            MaxMoney = membershipClass.MaxMoney,
            PercentDefault = membershipClass.PercentDefault,
            PercentBirthDate = membershipClass.PercentBirthDate,
            EffectiveYears = membershipClass.EffectiveYear ?? 1,
            MembershipBenefits = membershipClass.MembershipBenefits.Select(b => new MembershipBenefitResponse()
            {
                Id = b.Id.Value,
                Description = b.Description.Value,
                Title = b.Title.Value
            }).ToList()
        };
        return Result.Success(membershipClassResponse);
    }
}