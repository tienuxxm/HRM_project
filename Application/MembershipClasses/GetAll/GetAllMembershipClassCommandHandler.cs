using Application.Abstractions.Messaging;
using Application.MembershipClasses.GetOne;
using Domain.Abstractions;
using Domain.MembershipClasses;
using Microsoft.EntityFrameworkCore;

namespace Application.MembershipClasses.GetAll;

public class
    GetAllMembershipClassCommandHandler : ICommandHandler<GetAllMembershipClassCommand, List<MembershipClassResponse>>
{
    private readonly IMembershipClassRepository _membershipClassRepository;

    public GetAllMembershipClassCommandHandler(IMembershipClassRepository membershipClassRepository)
    {
        _membershipClassRepository = membershipClassRepository;
    }

    public async Task<Result<List<MembershipClassResponse>>> Handle(GetAllMembershipClassCommand request,
        CancellationToken cancellationToken)
    {
        var membershipClasses = await _membershipClassRepository
            .GetEntitiesAsQueryable()
            .OrderBy(x => x.Level)
            .Include(x => x.MembershipBenefits)
            .ToListAsync(cancellationToken);
        var membershipClassesResponse = membershipClasses.Select(x => new MembershipClassResponse()
        {
            Id = x.Id.Value,
            Level = x.Level.Value,
            ClassName = x.ClassName.Value,
            IsActive = x.IsActive,
            MaxMoney = x.MaxMoney,
            EffectiveYears = x.EffectiveYear ?? 1,
            MembershipBenefits = x.MembershipBenefits.Select(b => new MembershipBenefitResponse()
            {
                Description = b.Description.Value,
                Title = b.Title.Value
            }).ToList()
        }).ToList();
        return Result.Success(membershipClassesResponse);
    }
}